using Booking.Application.Dtos;
using Booking.Domain.Enums;
using Google.Cloud.Dialogflow.V2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;

namespace Booking.Application.Services
{
    public class DialogflowService : IDialogflowService
    {
        private readonly ITicketService _ticketService;
        private readonly ILogger<DialogflowService> _logger;
        private readonly SessionsClient _client;
        private readonly string _projectId;
        private readonly string _languageCode;

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
        }

        private static readonly string[] RequiredBookingFields =
        {
            ParameterKeys.DepartureStation,
            ParameterKeys.ArrivalStation,
            ParameterKeys.Date,
            ParameterKeys.Quantity,
            ParameterKeys.Time
        };

        public DialogflowService(
            ITicketService ticketService,
            IConfiguration configuration,
            ILogger<DialogflowService> logger)
        {
            _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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

        public async Task<DialogflowResponse> HandleBookingTicket(Dictionary<string, object> parameters)
        {
            _logger.LogInformation("Processing booking ticket request");
            string paramJson = JsonSerializer.Serialize(parameters, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            _logger.LogInformation("Booking ticket body: {Parameters}", paramJson);

            var missingFields = GetMissingFields(parameters);
            if (missingFields.Count > 0)
            {
                _logger.LogWarning("Missing required fields: {MissingFields}", string.Join(", ", missingFields));
                return CreateErrorResponse(string.Format(Messages.MissingFieldsTemplate, string.Join(", ", missingFields)));
            }

            var extractResult = ExtractBookingParameters(parameters);
            if (!extractResult.IsSuccess)
            {
                _logger.LogWarning("Parameter extraction failed: {Error}", extractResult.ErrorMessage);
                return CreateErrorResponse(extractResult.ErrorMessage);
            }

            var bookingInfo = extractResult.BookingInfo;
            _logger.LogInformation("Creating ticket for route: {Departure} -> {Arrival} on {Date}",
                bookingInfo.DepartureStation, bookingInfo.ArrivalStation, bookingInfo.Date);

            var ticket = await _ticketService.CreateTicketForDialogfowAsync(bookingInfo);

            return new DialogflowResponse
            {
                FulfillmentText = Messages.BookingInProgressMessage,
                Payload = new
                {
                    redirect = $"http://localhost:5173/booking/passenger-details?scheduleId={ticket.TrainScheduleId}&trainId={ticket.TrainId}&journeyDate={ticket.JourneyDate}",
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

        public async Task<string> DetectIntentAsync(string sessionId, string text)
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

            _logger.LogDebug("Detecting intent for session: {SessionId}, text: {Text}", sessionId, text);

            var response = await _client.DetectIntentAsync(request);
            return response.QueryResult.FulfillmentText;
        }

        private List<string> GetMissingFields(Dictionary<string, object> parameters)
        {
            return RequiredBookingFields
                .Where(field => !parameters.ContainsKey(field) ||
                               parameters[field] == null ||
                               string.IsNullOrWhiteSpace(parameters[field]?.ToString()))
                .ToList();
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

            if (!parameters.TryGetValue(ParameterKeys.PassengerName, out var passengerNameObj) ||
            passengerNameObj is not Dictionary<string, object> passengerNameDict ||
            !passengerNameDict.TryGetValue("name", out var nameObj) ||
            string.IsNullOrWhiteSpace(nameObj?.ToString()))
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
                PassengerName = nameObj.ToString().Trim(),
                PassengerEmail = passengerEmail?.Trim()
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
}
