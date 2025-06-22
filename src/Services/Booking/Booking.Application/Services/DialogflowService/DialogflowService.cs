using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using Booking.Application.Dtos;
using Booking.Domain.Enums;
using Common.Infrastructure.Utils;
using Common.Protos;
using Google.Cloud.Dialogflow.V2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Booking.Application.Services;
public class DialogflowService : IDialogflowService
{
    private readonly ITicketService _ticketService;
    private readonly IPassengerInfoService _passengerInfoService;
    private readonly ILogger<DialogflowService> _logger;
    private readonly SessionsClient _client;
    private readonly string _projectId;
    private readonly string _languageCode;
    private readonly PaymentGrpcService.PaymentGrpcServiceClient _paymentGrpcServiceClient;
    private static class Messages
    {
        public const string MissingFieldsTemplate = "Vui lòng cung cấp thêm: {0}";
        public const string InvalidDateMessage = "Ngày không hợp lệ. Vui lòng nhập lại.";
        public const string InvalidTimeMessage = "Giờ không hợp lệ. Vui lòng nhập lại.";
        public const string BookingInProgressMessage = "Vé của bạn đang được đặt. Vui lòng đợi trong giây lát...";
        public const string TicketNotFoundMessage = "Không tìm thấy vé nào với mã vé này.";
        public const string NoPassengerInfoMessage = "Không có thông tin hành khách";
    }

    private static class ParameterKeys
    {
        public const string DepartureStation = "departure_station";
        public const string ArrivalStation = "arrival_station";
        public const string Date = "date";
        public const string Time = "time";
        public const string Quantity = "quantity";
        public const string TicketNumber = "ticket_number";
        public const string PassengerName = "passenger_name";
        public const string PassengerEmail = "passenger_email";
        public const string PassengerIdentity = "passenger_identity";
        public const string PassengerPhone = "passenger_phone";
        public const string PaymentType = "payment_type";
    }

    public DialogflowService(
        ITicketService ticketService,
        IPassengerInfoService passengerInfoService,
        IConfiguration configuration,
        ILogger<DialogflowService> logger,
        PaymentGrpcService.PaymentGrpcServiceClient paymentGrpcServiceClient)
    {
        _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
        _passengerInfoService = passengerInfoService ?? throw new ArgumentNullException(nameof(passengerInfoService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _paymentGrpcServiceClient = paymentGrpcServiceClient;
        _projectId = configuration["Dialogflow:ProjectId"]
            ?? throw new InvalidOperationException("Dialogflow:ProjectId not found in configuration.");

        _languageCode = configuration["Dialogflow:LanguageCode"] ?? "vi";

        var jsonCredentials = configuration["Dialogflow:JSON"]
            ?? throw new InvalidOperationException("Dialogflow:JSON not found in configuration.");

        try
        {
            var builder = new SessionsClientBuilder
            {
                JsonCredentials = jsonCredentials
            };
            _client = builder.Build();

            _logger.LogInformation("DialogflowService initialized successfully for project: {ProjectId}", _projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize DialogflowService");
            throw;
        }
    }

    public async Task<DialogflowResponse> HandleBookingTicket(Dictionary<string, object> parameters, string session)
    {

        var extractResult = ExtractBookingParameters(parameters);
        if (!extractResult.IsSuccess)
        {
            _logger.LogWarning("Parameter extraction failed: {Error}", extractResult.ErrorMessage);
            return CreateErrorResponse(extractResult.ErrorMessage);
        }

        var bookingInfo = extractResult.BookingInfo;

        var ticket = await _ticketService.CreateTicketForDialogfowAsync(bookingInfo);

        if (ticket == null)
        {
            _logger.LogError("Failed to create ticket for booking info: {BookingInfo}", bookingInfo);
            return CreateErrorResponse("Không thể tạo vé. Vui lòng thử lại sau.");
        }

        string firstName = null;
        string lastName = null;

        if (!string.IsNullOrWhiteSpace(extractResult.BookingInfo.PassengerName))
        {
            var nameParts = extractResult.BookingInfo.PassengerName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            firstName = nameParts.Length > 0 ? nameParts[0] : null;
            lastName = nameParts.Length > 1 ? nameParts[1] : null;
        }

        var addPassengerDto = new AddPassengerInfoDto
        {
            IsMainPassenger = true,
            AgeGroup = AgeGroupEnum.Adult,
            FirstName = firstName,
            LastName = lastName,
            Email = parameters[ParameterKeys.PassengerEmail]?.ToString(),
            PhoneNumber = parameters[ParameterKeys.PassengerPhone]?.ToString(),
            IdentityNumber = parameters[ParameterKeys.PassengerIdentity]?.ToString(),
            TicketSeatId = ticket.TicketSeats?.FirstOrDefault()?.Id
        };

        var addPassengerDetails = new AddPassengerDetailsDto
        {
            TicketId = ticket.Id,
            PassengerInfos = new List<AddPassengerInfoDto> { addPassengerDto }
        };

        await _passengerInfoService.AddPassengerDetailsAsync(addPassengerDetails);
        var request = new CreatePaymentRequest
        {
            BookingOrderId = ticket.BookingOrderId.ToString(),
            PaymentType = int.Parse(parameters[ParameterKeys.PaymentType].ToString()),
        };

        var response = await _paymentGrpcServiceClient.CreatePaymentAsync(request);

        return new DialogflowResponse
        {
            FulfillmentText = Messages.BookingInProgressMessage,
            Payload = new
            {
                redirect = response.PaymentUrl,
                bookingInfo
            }
        };
    }

    public async Task<DialogflowResponse> HandleSearchTicket(Dictionary<string, object> parameters)
    {
        var ticketNumber = ExtractStringParameter(parameters, ParameterKeys.TicketNumber);
        if (string.IsNullOrWhiteSpace(ticketNumber))
        {
            _logger.LogWarning("Ticket number is missing or empty");
            return CreateErrorResponse("Vui lòng cung cấp mã vé để tìm kiếm.");
        }

        _logger.LogInformation("Searching for ticket: {TicketNumber}", ticketNumber);

        var ticket = await _ticketService.GetTicketByTicketNumberAsync(ticketNumber);
        var message = GenerateTicketInfoMessage(ticket);

        return new DialogflowResponse
        {
            FulfillmentText = message
        };
    }

    public async Task<DialogflowResponse> HandleCheckTicketAvailability(Dictionary<string, object> parameters, string session)
    {
        var departure = ExtractStringParameter(parameters, ParameterKeys.DepartureStation);
        var arrival = ExtractStringParameter(parameters, ParameterKeys.ArrivalStation);
        var quantityStr = ExtractStringParameter(parameters, ParameterKeys.Quantity);
        var dateStr = ExtractStringParameter(parameters, ParameterKeys.Date);
        var timeStr = ExtractStringParameter(parameters, ParameterKeys.Time);

        if (!double.TryParse(parameters[ParameterKeys.Quantity]?.ToString(), out var quantity) ||
            quantity <= 0 ||
            quantity % 1 != 0)
        {
            return CreateErrorResponse("Số lượng không hợp lệ.");
        }

        if (!DateTime.TryParse(dateStr, out var date))
        {
            return CreateErrorResponse(Messages.InvalidDateMessage);
        }

        if (!DateTime.TryParse(timeStr, out var time))
        {
            return CreateErrorResponse(Messages.InvalidTimeMessage);
        }

        var availableTrips = await _ticketService.CheckTrainAvailabilityAsync(new CheckTrainAvailabilityDto
        {
            DepartureStation = departure,
            ArrivalStation = arrival,
            Date = date.Date,
            Time = time.TimeOfDay,
            Quantity = (int)quantity
        });

        if (availableTrips)
        {
            return new DialogflowResponse
            {
                FulfillmentText = "Chuyến tàu phù hợp đã được tìm thấy. Quý khách vui lòng cung cấp thông tin hành khách: họ và tên, email, số CCCD/Passport và số điện thoại để tiến hành đặt vé",
                OutputContexts = new List<DialogflowContext>
                {
                    new DialogflowContext
                    {
                        Name = $"{session}/contexts/available_route_confirmed",
                        LifespanCount = 5,
                        Parameters = new Dictionary<string, object>
                        {
                            ["departure_station"] = departure,
                            ["arrival_station"] = arrival,
                            ["date"] = date,
                            ["time"] = time,
                            ["quantity"] = quantity
                        }
                    }
                }
            };
        }
        else
        {
            return new DialogflowResponse
            {
                FulfillmentText = "Chúng tôi rất tiếc, hiện không tìm thấy chuyến tàu phù hợp với thông tin quý khách đã cung cấp. Quý khách vui lòng thử lại với thời gian khác."
            };
        }
    }

    public async Task<(string FulfillmentText, object Payload)> DetectIntentWithPayloadAsync(string sessionId, string text)
    {
        var sessionName = SessionName.FromProjectSession(_projectId, sessionId);

        var request = new DetectIntentRequest
        {
            SessionAsSessionName = sessionName,
            QueryInput = new QueryInput
            {
                Text = new TextInput
                {
                    Text = text,
                    LanguageCode = _languageCode
                }
            }
        };

        var response = await _client.DetectIntentAsync(request);
        return (response.QueryResult.FulfillmentText, response.QueryResult.WebhookPayload?.ToDictionary());
    }

    private (bool IsSuccess, DialogflowCreateTicketRequest BookingInfo, string ErrorMessage) ExtractBookingParameters(
Dictionary<string, object> parameters)
    {
        var departureStation = ExtractStringParameter(parameters, ParameterKeys.DepartureStation);
        var arrivalStation = ExtractStringParameter(parameters, ParameterKeys.ArrivalStation);

        if (!double.TryParse(parameters[ParameterKeys.Quantity]?.ToString(), out var quantity) ||
            quantity <= 0 ||
            quantity % 1 != 0)
        {
            return (false, null, "Số lượng vé không hợp lệ.");
        }

        var dateStr = parameters[ParameterKeys.Date]?.ToString();
        if (!DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return (false, null, Messages.InvalidDateMessage);
        }

        var timeStr = parameters[ParameterKeys.Time]?.ToString();
        if (!DateTime.TryParse(timeStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var timeDateTime))
        {
            return (false, null, Messages.InvalidTimeMessage);
        }

        string passengerName = null;
        if (parameters.TryGetValue(ParameterKeys.PassengerName, out var passengerNameObj))
        {
            switch (passengerNameObj)
            {
                case JsonElement jsonElement when jsonElement.ValueKind == JsonValueKind.Object:
                    if (jsonElement.TryGetProperty("name", out var nameProp))
                    {
                        passengerName = nameProp.GetString();
                    }
                    break;

                case Dictionary<string, object> dict:
                    if (dict.TryGetValue("name", out var nameObj))
                    {
                        passengerName = nameObj?.ToString();
                    }
                    break;

                default:
                    passengerName = passengerNameObj?.ToString();
                    break;
            }
        }

        if (string.IsNullOrWhiteSpace(passengerName))
        {
            return (false, null, "Tên hành khách không hợp lệ hoặc bị thiếu.");
        }

        var passengerEmail = parameters[ParameterKeys.PassengerEmail]?.ToString();
        if (string.IsNullOrWhiteSpace(passengerEmail))
        {
            return (false, null, "Email hành khách không được để trống.");
        }

        var bookingInfo = new DialogflowCreateTicketRequest
        {
            DepartureStation = departureStation,
            ArrivalStation = arrivalStation,
            Quantity = (int)quantity,
            Date = date.Date,
            Time = timeDateTime.TimeOfDay,
            PassengerName = passengerName?.Trim(),
            PassengerEmail = passengerEmail?.Trim(),
            TicketType = TicketTypeEnum.Normal
        };

        return (true, bookingInfo, null);
    }

    private string ExtractStringParameter(Dictionary<string, object> parameters, string key)
    {
        return parameters.TryGetValue(key, out var value) ? value?.ToString()?.Trim() : null;
    }

    private DialogflowResponse CreateErrorResponse(string message)
    {
        return new DialogflowResponse
        {
            FulfillmentText = message
        };
    }

    public string GenerateTicketInfoMessage(TicketDto ticket)
    {
        if (ticket == null)
        {
            return Messages.TicketNotFoundMessage;
        }

        var passengerNames = GetPassengerNames(ticket);
        var seatDetails = ticket.SeatInformations?.Select(seat =>
        {
            var train = seat.TrainCar?.Train?.TrainName ?? "N/A";
            var carNumber = seat.TrainCar?.CarNumber?.ToString() ?? "N/A";
            var seatType = GetSeatTypeText(seat.TrainCar?.SeatType ?? 0);
            var seatNumber = seat.SeatNumber.ToString() ?? "N/A";

            return $"🚂 Tàu: {train} - 🚃 Toa: {carNumber} - 💺 Ghế: {seatNumber} ({seatType})";
        }) ?? new List<string> { "Không có thông tin ghế." };

        var statusText = GetStatusText(ticket.Status);
        var bookingDateText = ticket.BookingDate.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

        var message = $"🎫 **THÔNG TIN VÉ TÀU**<br/>" +
                     $"━━━━━━━━━━━<br/>" +
                     $"🆔 Mã vé: {ticket.TicketNumber}<br/>" +
                     string.Join("<br/>", seatDetails) + "<br/>" +
                     $"📊 Số lượng ghế: {ticket.SeatInformations?.Count ?? 0}<br/>" +
                     $"🗓️ Ngày khởi hành: {ticket.JourneyDate:dd/MM/yyyy}<br/>" +
                     $"📅 Ngày đặt vé: {bookingDateText}<br/>" +
                     $"💵 Tổng tiền: {ticket.TotalPrice:N0} VNĐ<br/>" +
                     $"📌 Trạng thái: {statusText}<br/>" +
                     $"👤 Hành khách: {passengerNames}";

        if (!string.IsNullOrWhiteSpace(ticket.Remarks))
        {
            message += $"<br/>📝 Ghi chú: {ticket.Remarks}";
        }

        return message;
    }

    private string GetPassengerNames(TicketDto ticket)
    {
        if (ticket.PassengerDetails == null || !ticket.PassengerDetails.Any())
        {
            return Messages.NoPassengerInfoMessage;
        }

        return string.Join(", ", ticket.PassengerDetails
            .Where(p => !string.IsNullOrWhiteSpace(p.FirstName) || !string.IsNullOrWhiteSpace(p.LastName))
            .Select(p => $"{p.FirstName?.Trim()} {p.LastName?.Trim()}".Trim()));
    }

    private static string GetSeatTypeText(int seatType)
    {
        return seatType switch
        {
            1 => "Ghế cứng",
            2 => "Ghế mềm",
            3 => "Giường nằm cứng",
            4 => "Giường nằm mềm",
            5 => "VIP",
            _ => "Không xác định"
        };
    }

    private static string GetStatusText(TicketStatusEnum status)
    {
        return status switch
        {
            TicketStatusEnum.Active => "✅ Đang hoạt động",
            TicketStatusEnum.Used => "🎫 Đã sử dụng",
            TicketStatusEnum.Cancelled => "❌ Đã hủy",
            TicketStatusEnum.Expired => "⌛ Hết hạn",
            TicketStatusEnum.Refunded => "💰 Đã hoàn tiền",
            TicketStatusEnum.UnPaid => "💳 Chưa thanh toán",
            _ => status.ToString()
        };
    }
}
