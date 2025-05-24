using Booking.Application.Dtos;
using Booking.Domain.Enums;
using Google.Cloud.Dialogflow.V2;
using Microsoft.Extensions.Configuration;

namespace Booking.Application.Services
{
    public class DialogflowService : IDialogflowService
    {
        private readonly ITicketService _ticketService;
        private readonly string _projectId;
        private readonly string _jsonCredentials;
        private readonly string _languageCode;
        private readonly SessionsClient _client;
        public DialogflowService(ITicketService ticketService, IConfiguration configuration)
        {
            _ticketService = ticketService;
            _projectId = configuration["Dialogflow:ProjectId"]!;
            _languageCode = configuration["Dialogflow:LanguageCode"] ?? "vi";
            _jsonCredentials = configuration["Dialogflow:JSON"]
                               ?? throw new InvalidOperationException(
                                   "Secret Dialogflow:JSON not found in configuration.");

            var builder = new SessionsClientBuilder
            {
                JsonCredentials = _jsonCredentials
            };
            _client = builder.Build();
        }
        public Task<DialogflowResponse> HandleBookingTicket(Dictionary<string, object> parameters)
        {
            var missingFields = CheckMissingFields(parameters);
            if (missingFields.Any())
            {
                return Task.FromResult(new DialogflowResponse
                {
                    FulfillmentText = $"Vui lòng cung cấp thêm: {string.Join(", ", missingFields)}"
                });
            }

            try
            {
                var departureStation = parameters["departure_station"]?.ToString();
                var arrivalStation = parameters["arrival_station"]?.ToString();

                // Parse quantity safely
                var quantityObj = parameters["quantity"];
                var quantity = Convert.ToInt32(quantityObj); // handles both int and double

                // Parse date
                var dateStr = parameters["date"]?.ToString();
                if (!DateTime.TryParse(dateStr, out var date))
                {
                    return Task.FromResult(new DialogflowResponse
                    {
                        FulfillmentText = "Ngày không hợp lệ. Vui lòng nhập lại."
                    });
                }

                // Parse time
                var timeStr = parameters["time"]?.ToString();
                if (!DateTime.TryParse(timeStr, out var timeDateTime))
                {
                    return Task.FromResult(new DialogflowResponse
                    {
                        FulfillmentText = "Giờ không hợp lệ. Vui lòng nhập lại."
                    });
                }

                var bookingInfo = new DialogflowCreateTicketRequest
                {
                    DepartureStation = departureStation,
                    ArrivalStation = arrivalStation,
                    Quantity = quantity,
                    Date = date.Date,
                    Time = timeDateTime.TimeOfDay
                };

                var result = new DialogflowResponse
                {
                    FulfillmentText = "Vé của bạn đang được đặt. Vui lòng đợi trong giây lát...",
                    Payload = new
                    {
                        redirect = "http://localhost:5173/ok",
                        bookingInfo
                    }
                };

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                return Task.FromResult(new DialogflowResponse
                {
                    FulfillmentText = $"Đã xảy ra lỗi khi xử lý yêu cầu: {ex.Message}"
                });
            }
        }

        public async Task<DialogflowResponse> HandleSearchTicket(Dictionary<string, object> parameters)
        {
            var ticketNumber = parameters["ticket_number"]?.ToString();
            var ticket = await _ticketService.GetTicketByTicketNumberAsync(ticketNumber);
            var message = GenerateTicketInfoMessage(ticket);

            var result = new DialogflowResponse
            {
                FulfillmentText = message
            };

            return result;
        }

        private List<string> CheckMissingFields(Dictionary<string, object> parameters)
        {
            var requiredFields = new[] { "departure_station", "arrival_station", "date", "quantity", "time" };
            return requiredFields.Where(f => !parameters.ContainsKey(f) || parameters[f] == null).ToList();
        }

        public string GenerateTicketInfoMessage(TicketDto ticket)
        {
            if (ticket == null)
            {
                return "Không tìm thấy vé nào với mã vé này.";
            }

            var passengerNames = ticket.PassengerDetails != null && ticket.PassengerDetails.Any()
                ? string.Join(", ", ticket.PassengerDetails.Select(p => $"{p.FirstName} {p.LastName}"))
                : "Không có thông tin hành khách";

            var trainInfo = ticket.SeatInformation?.TrainCar?.Train?.TrainName ?? "N/A";

            var carNumber = ticket.SeatInformation?.TrainCar?.CarNumber?.ToString() ?? "N/A";
            var seatTypeText = GetSeatTypeText(ticket.SeatInformation?.TrainCar?.SeatType ?? 0);

            var seatNumber = ticket.SeatInformation?.SeatNumber.ToString() ?? "N/A";
            var totalSeats = ticket.TicketSeats?.Count ?? 0;

            var statusText = GetStatusText(ticket.Status);
            var bookingDateText = ticket.BookingDate.ToString("dd/MM/yyyy HH:mm");

            var message = $"🎫 **THÔNG TIN VÉ TÀU**</br>" +
                          $"━━━━━━━━━━━━━━━━━━━━━━</br>" +
                          $"🆔 Mã vé: {ticket.TicketNumber}</br>" +
                          $"🚂 Tàu: {trainInfo}</br>" +
                          $"🚃 Toa: {carNumber}</br>" +
                          $"💺 Ghế: {seatNumber} ({seatTypeText})</br>" +
                          $"📊 Số lượng ghế: {totalSeats}</br>" +
                          $"🗓️ Ngày khởi hành: {ticket.JourneyDate:dd/MM/yyyy}</br>" +
                          $"📅 Ngày đặt vé: {bookingDateText}</br>" +
                          $"💵 Tổng tiền: {ticket.TotalPrice:N0} VNĐ</br>" +
                          $"📌 Trạng thái: {statusText}</br>" +
                          $"👤 Hành khách: {passengerNames}";

            // Thêm ghi chú nếu có
            if (!string.IsNullOrEmpty(ticket.Remarks))
            {
                message += $"\n📝 Ghi chú: {ticket.Remarks}";
            }

            return message;
        }


        private string GetSeatTypeText(int seatType)
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

        private string GetStatusText(TicketStatusEnum status)
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

        public async Task<string> DetectIntentAsync(string sessionId, string text)
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
            return response.QueryResult.FulfillmentText;
        }
    }
}
