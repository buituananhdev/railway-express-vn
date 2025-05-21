using Booking.Application.Dtos;

namespace Booking.Application.Services
{
    public class DialogflowService : IDialogflowService
    {
        private readonly ITicketService _ticketService;

        public DialogflowService(ITicketService ticketService)
        {
            _ticketService = ticketService;
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
            if(ticket == null)
            {
                return "Không tìm thấy vé nào với mã vé này.";
            }

            var passengerNames = ticket.PassengerDetails != null && ticket.PassengerDetails.Any()
                ? string.Join(", ", ticket.PassengerDetails.Select(p => $"{p.FirstName + " " + p.LastName}"))
                : "Không có thông tin hành khách.";

            return
                $"🎫 Mã vé: {ticket.TicketNumber}\n" +
                $"🗓️ Ngày khởi hành: {ticket.JourneyDate:dd/MM/yyyy}\n" +
                $"💺 Số ghế: {(ticket.SeatIds?.Count ?? 0)}\n" +
                $"💵 Tổng tiền: {ticket.TotalPrice:N0} VNĐ\n" +
                $"📌 Trạng thái: {ticket.Status.ToString()}\n" +
                $"👤 Hành khách: {passengerNames}";
        }
    }
}
