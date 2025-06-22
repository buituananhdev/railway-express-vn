using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
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

    // Regular expressions for validation
    private static readonly Regex EmailRegex = new(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);
    private static readonly Regex PhoneRegex = new(@"^(\+84|84|0)([1-9][0-9]{8,9})$", RegexOptions.Compiled);
    private static readonly Regex IdentityRegex = new(@"^[0-9]{9,12}$", RegexOptions.Compiled);
    private static readonly Regex NameRegex = new(@"^[a-zA-ZÀ-ỹ\s]{2,50}$", RegexOptions.Compiled);

    private static class Messages
    {
        public const string MissingFieldsTemplate = "Vui lòng cung cấp thêm: {0}";
        public const string InvalidDateMessage = "Ngày không hợp lệ. Vui lòng nhập lại.";
        public const string InvalidTimeMessage = "Giờ không hợp lệ. Vui lòng nhập lại.";
        public const string BookingInProgressMessage = "Vé của bạn đang được đặt. Vui lòng đợi trong giây lát...";
        public const string TicketNotFoundMessage = "Không tìm thấy vé nào với mã vé này.";
        public const string NoPassengerInfoMessage = "Không có thông tin hành khách";
        public const string InvalidEmailMessage = "Email không hợp lệ. Vui lòng nhập email đúng định dạng.";
        public const string InvalidPhoneMessage = "Số điện thoại không hợp lệ. Vui lòng nhập số điện thoại Việt Nam hợp lệ.";
        public const string InvalidIdentityMessage = "Số CCCD/Passport không hợp lệ. Vui lòng nhập từ 9-12 chữ số.";
        public const string InvalidNameMessage = "Họ tên không hợp lệ. Vui lòng nhập từ 2-50 ký tự, chỉ chứa chữ cái và khoảng trắng.";
        public const string InvalidQuantityMessage = "Số lượng vé phải là số nguyên dương từ 1-10.";
        public const string InvalidStationMessage = "Ga đi hoặc ga đến không hợp lệ.";
        public const string SameStationMessage = "Ga đi và ga đến không được trùng nhau.";
        public const string PastDateMessage = "Ngày khởi hành không được trong quá khứ.";
        public const string FarFutureDateMessage = "Ngày khởi hành không được quá 90 ngày kể từ hôm nay.";
        public const string InvalidTicketNumberMessage = "Mã vé không hợp lệ. Vui lòng kiểm tra lại.";
        public const string InvalidPaymentTypeMessage = "Loại thanh toán không hợp lệ.";
        public const string SessionExpiredMessage = "Phiên làm việc đã hết hạn. Vui lòng bắt đầu lại.";
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
        _paymentGrpcServiceClient = paymentGrpcServiceClient ?? throw new ArgumentNullException(nameof(paymentGrpcServiceClient));

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
        try
        {
            // Validate session
            if (string.IsNullOrWhiteSpace(session))
            {
                _logger.LogWarning("Session is null or empty");
                return CreateErrorResponse(Messages.SessionExpiredMessage);
            }

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

            // Validate payment type
            var paymentTypeValidation = ValidatePaymentType(parameters[ParameterKeys.PaymentType]?.ToString());
            if (!paymentTypeValidation.IsValid)
            {
                return CreateErrorResponse(paymentTypeValidation.ErrorMessage);
            }

            var request = new CreatePaymentRequest
            {
                BookingOrderId = ticket.BookingOrderId.ToString(),
                PaymentType = paymentTypeValidation.PaymentType,
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling booking ticket");
            return CreateErrorResponse("Đã xảy ra lỗi trong quá trình đặt vé. Vui lòng thử lại sau.");
        }
    }

    public async Task<DialogflowResponse> HandleSearchTicket(Dictionary<string, object> parameters)
    {
        try
        {
            var ticketNumber = ExtractStringParameter(parameters, ParameterKeys.TicketNumber);
            var validationResult = ValidateTicketNumber(ticketNumber);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Invalid ticket number: {TicketNumber}", ticketNumber);
                return CreateErrorResponse(validationResult.ErrorMessage);
            }

            _logger.LogInformation("Searching for ticket: {TicketNumber}", ticketNumber);

            var ticket = await _ticketService.GetTicketByTicketNumberAsync(ticketNumber);
            var message = GenerateTicketInfoMessage(ticket);

            return new DialogflowResponse
            {
                FulfillmentText = message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching ticket");
            return CreateErrorResponse("Đã xảy ra lỗi khi tìm kiếm vé. Vui lòng thử lại sau.");
        }
    }

    public async Task<DialogflowResponse> HandleCheckTicketAvailability(Dictionary<string, object> parameters, string session)
    {
        try
        {
            // Validate session
            if (string.IsNullOrWhiteSpace(session))
            {
                _logger.LogWarning("Session is null or empty");
                return CreateErrorResponse(Messages.SessionExpiredMessage);
            }

            var departure = ExtractStringParameter(parameters, ParameterKeys.DepartureStation);
            var arrival = ExtractStringParameter(parameters, ParameterKeys.ArrivalStation);
            var quantityStr = ExtractStringParameter(parameters, ParameterKeys.Quantity);
            var dateStr = ExtractStringParameter(parameters, ParameterKeys.Date);
            var timeStr = ExtractStringParameter(parameters, ParameterKeys.Time);

            // Validate stations
            var stationValidation = ValidateStations(departure, arrival);
            if (!stationValidation.IsValid)
            {
                return CreateErrorResponse(stationValidation.ErrorMessage);
            }

            // Validate quantity
            var quantityValidation = ValidateQuantity(quantityStr);
            if (!quantityValidation.IsValid)
            {
                return CreateErrorResponse(quantityValidation.ErrorMessage);
            }

            // Validate date
            var dateValidation = ValidateDate(dateStr);
            if (!dateValidation.IsValid)
            {
                return CreateErrorResponse(dateValidation.ErrorMessage);
            }

            // Validate time
            var timeValidation = ValidateTime(timeStr);
            if (!timeValidation.IsValid)
            {
                return CreateErrorResponse(timeValidation.ErrorMessage);
            }

            var dateTimeValidation = ValidateDateTime(dateValidation.Date, timeValidation.Time);
            if (!dateTimeValidation.IsValid)
            {
                return CreateErrorResponse(dateTimeValidation.ErrorMessage);
            }

            var availableTrips = await _ticketService.CheckTrainAvailabilityAsync(new CheckTrainAvailabilityDto
            {
                DepartureStation = departure,
                ArrivalStation = arrival,
                Date = dateValidation.Date.Date,
                Time = timeValidation.Time.TimeOfDay,
                Quantity = quantityValidation.Quantity
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
                                ["date"] = dateValidation.Date,
                                ["time"] = timeValidation.Time,
                                ["quantity"] = quantityValidation.Quantity
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking ticket availability");
            return CreateErrorResponse("Đã xảy ra lỗi khi kiểm tra tình trạng vé. Vui lòng thử lại sau.");
        }
    }

    public async Task<(string FulfillmentText, object Payload)> DetectIntentWithPayloadAsync(string sessionId, string text)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("Session ID cannot be null or empty", nameof(sessionId));

            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text cannot be null or empty", nameof(text));

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting intent");
            return ("Đã xảy ra lỗi khi xử lý yêu cầu. Vui lòng thử lại sau.", null);
        }
    }

    private (bool IsSuccess, DialogflowCreateTicketRequest BookingInfo, string ErrorMessage) ExtractBookingParameters(
        Dictionary<string, object> parameters)
    {
        try
        {
            if (parameters == null)
            {
                return (false, null, "Không có thông tin đặt vé.");
            }

            var departureStation = ExtractStringParameter(parameters, ParameterKeys.DepartureStation);
            var arrivalStation = ExtractStringParameter(parameters, ParameterKeys.ArrivalStation);

            // Validate stations
            var stationValidation = ValidateStations(departureStation, arrivalStation);
            if (!stationValidation.IsValid)
            {
                return (false, null, stationValidation.ErrorMessage);
            }

            // Validate quantity
            var quantityValidation = ValidateQuantity(parameters[ParameterKeys.Quantity]?.ToString());
            if (!quantityValidation.IsValid)
            {
                return (false, null, quantityValidation.ErrorMessage);
            }

            // Validate date
            var dateStr = parameters[ParameterKeys.Date]?.ToString();
            var dateValidation = ValidateDate(dateStr);
            if (!dateValidation.IsValid)
            {
                return (false, null, dateValidation.ErrorMessage);
            }

            // Validate time
            var timeStr = parameters[ParameterKeys.Time]?.ToString();
            var timeValidation = ValidateTime(timeStr);
            if (!timeValidation.IsValid)
            {
                return (false, null, timeValidation.ErrorMessage);
            }

            var dateTimeValidation = ValidateDateTime(dateValidation.Date, timeValidation.Time);
            if (!dateTimeValidation.IsValid)
            {
                return (false, null, dateTimeValidation.ErrorMessage);
            }

            // Extract and validate passenger name
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

            var nameValidation = ValidateName(passengerName);
            if (!nameValidation.IsValid)
            {
                return (false, null, nameValidation.ErrorMessage);
            }

            // Validate email
            var passengerEmail = parameters[ParameterKeys.PassengerEmail]?.ToString();
            var emailValidation = ValidateEmail(passengerEmail);
            if (!emailValidation.IsValid)
            {
                return (false, null, emailValidation.ErrorMessage);
            }

            // Validate phone (optional for booking, but if provided should be valid)
            var passengerPhone = parameters[ParameterKeys.PassengerPhone]?.ToString();
            if (!string.IsNullOrWhiteSpace(passengerPhone))
            {
                var phoneValidation = ValidatePhone(passengerPhone);
                if (!phoneValidation.IsValid)
                {
                    return (false, null, phoneValidation.ErrorMessage);
                }
            }

            // Validate identity (optional for booking, but if provided should be valid)
            var passengerIdentity = parameters[ParameterKeys.PassengerIdentity]?.ToString();
            if (!string.IsNullOrWhiteSpace(passengerIdentity))
            {
                var identityValidation = ValidateIdentity(passengerIdentity);
                if (!identityValidation.IsValid)
                {
                    return (false, null, identityValidation.ErrorMessage);
                }
            }

            var bookingInfo = new DialogflowCreateTicketRequest
            {
                DepartureStation = departureStation,
                ArrivalStation = arrivalStation,
                Quantity = quantityValidation.Quantity,
                Date = dateValidation.Date.Date,
                Time = timeValidation.Time.TimeOfDay,
                PassengerName = passengerName?.Trim(),
                PassengerEmail = passengerEmail?.Trim(),
                TicketType = TicketTypeEnum.Normal
            };

            return (true, bookingInfo, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting booking parameters");
            return (false, null, "Đã xảy ra lỗi khi xử lý thông tin đặt vé.");
        }
    }

    // Validation methods
    private (bool IsValid, string ErrorMessage) ValidateStations(string departure, string arrival)
    {
        if (string.IsNullOrWhiteSpace(departure) || string.IsNullOrWhiteSpace(arrival))
        {
            return (false, Messages.InvalidStationMessage);
        }

        if (departure.Trim().Equals(arrival.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return (false, Messages.SameStationMessage);
        }

        return (true, null);
    }

    private (bool IsValid, int Quantity, string ErrorMessage) ValidateQuantity(string quantityStr)
    {
        if (!double.TryParse(quantityStr, out var quantity) ||
            quantity <= 0 ||
            quantity % 1 != 0 ||
            quantity > 10)
        {
            return (false, 0, Messages.InvalidQuantityMessage);
        }

        return (true, (int)quantity, null);
    }

    private (bool IsValid, DateTime Date, string ErrorMessage) ValidateDate(string dateStr)
    {
        if (!DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return (false, DateTime.MinValue, Messages.InvalidDateMessage);
        }

        var today = DateTime.Today;
        if (date.Date < today)
        {
            return (false, DateTime.MinValue, Messages.PastDateMessage);
        }

        if (date.Date > today.AddDays(90))
        {
            return (false, DateTime.MinValue, Messages.FarFutureDateMessage);
        }

        return (true, date, null);
    }

    private (bool IsValid, DateTime Time, string ErrorMessage) ValidateTime(string timeStr)
    {
        if (!DateTime.TryParse(timeStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var timeDateTime))
        {
            return (false, DateTime.MinValue, Messages.InvalidTimeMessage);
        }

        return (true, timeDateTime, null);
    }

    private (bool IsValid, string ErrorMessage) ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return (false, "Tên hành khách không được để trống.");
        }

        if (!NameRegex.IsMatch(name.Trim()))
        {
            return (false, Messages.InvalidNameMessage);
        }

        return (true, null);
    }

    private (bool IsValid, string ErrorMessage) ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return (false, "Email hành khách không được để trống.");
        }

        if (!EmailRegex.IsMatch(email.Trim()))
        {
            return (false, Messages.InvalidEmailMessage);
        }

        return (true, null);
    }

    private (bool IsValid, string ErrorMessage) ValidatePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return (false, "Số điện thoại không được để trống.");
        }

        if (!PhoneRegex.IsMatch(phone.Trim()))
        {
            return (false, Messages.InvalidPhoneMessage);
        }

        return (true, null);
    }

    private (bool IsValid, string ErrorMessage) ValidateIdentity(string identity)
    {
        if (string.IsNullOrWhiteSpace(identity))
        {
            return (false, "Số CCCD/Passport không được để trống.");
        }

        if (!IdentityRegex.IsMatch(identity.Trim()))
        {
            return (false, Messages.InvalidIdentityMessage);
        }

        return (true, null);
    }

    private (bool IsValid, string ErrorMessage) ValidateTicketNumber(string ticketNumber)
    {
        if (string.IsNullOrWhiteSpace(ticketNumber))
        {
            return (false, "Vui lòng cung cấp mã vé để tìm kiếm.");
        }

        // Basic validation for ticket number format
        if (ticketNumber.Trim().Length < 6 || ticketNumber.Trim().Length > 20)
        {
            return (false, Messages.InvalidTicketNumberMessage);
        }

        return (true, null);
    }

    private (bool IsValid, int PaymentType, string ErrorMessage) ValidatePaymentType(string paymentTypeStr)
    {
        if (!int.TryParse(paymentTypeStr, out var paymentType) || paymentType < 1 || paymentType > 10)
        {
            return (false, 0, Messages.InvalidPaymentTypeMessage);
        }

        return (true, paymentType, null);
    }

    private (bool IsValid, string ErrorMessage) ValidateDateTime(DateTime date, DateTime time)
    {
        var currentDate = DateTime.Today;
        var now = DateTime.Now;

        if (date.Date < currentDate)
        {
            return (false, Messages.PastDateMessage);
        }

        if (date.Date == currentDate && time.TimeOfDay <= now.TimeOfDay)
        {
            return (false, "Giờ khởi hành phải lớn hơn giờ hiện tại.");
        }

        return (true, null);
    }

    private string ExtractStringParameter(Dictionary<string, object> parameters, string key)
    {
        if (parameters == null) return null;
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

            return $"• Tàu: <strong>{train}</strong> – Toa: <strong>{carNumber}</strong> – Ghế: <strong>{seatNumber}</strong> ({seatType})";
        }) ?? new List<string> { "Không có thông tin ghế." };

        var statusText = GetStatusText(ticket.Status);
        var bookingDateText = ticket.BookingDate.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
        var journeyDateText = ticket.JourneyDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        var totalPriceText = $"{ticket.TotalPrice:N0} VNĐ";
        var seatCount = ticket.SeatInformations?.Count ?? 0;

        var message = $@"
        <strong>📄 THÔNG TIN VÉ ĐIỆN TỬ</strong><br/>
        <strong>Mã vé:</strong> {ticket.TicketNumber}<br/>
        {string.Join("<br/>", seatDetails)}<br/>
        <strong>Số lượng ghế:</strong> {seatCount}<br/>
        <strong>Ngày khởi hành:</strong> {journeyDateText}<br/>
        <strong>Ngày đặt vé:</strong> {bookingDateText}<br/>
        <strong>Tổng tiền:</strong> {totalPriceText}<br/>
        <strong>Trạng thái:</strong> {statusText}<br/>
        <strong>Hành khách:</strong> {passengerNames}";

        if (!string.IsNullOrWhiteSpace(ticket.Remarks))
        {
            message += $"<br/><strong>Ghi chú:</strong> {ticket.Remarks}";
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
            TicketStatusEnum.Active => "Đang hoạt động",
            TicketStatusEnum.Used => "Đã sử dụng",
            TicketStatusEnum.Cancelled => "Đã hủy",
            TicketStatusEnum.Expired => "Hết hạn",
            TicketStatusEnum.Refunded => "Đã hoàn tiền",
            TicketStatusEnum.UnPaid => "Chưa thanh toán",
            _ => status.ToString()
        };
    }
}
