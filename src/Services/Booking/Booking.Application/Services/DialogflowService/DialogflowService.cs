using Booking.Application.Dtos;

namespace Booking.Application.Services
{
    public class DialogflowService : IDialogflowService
    {
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

            var bookingInfo = new DialogflowCreateTicketRequest
            {
                DepartureStation = parameters["departure_station"]?.ToString(),
                ArrivalStation = parameters["arrival_station"]?.ToString(),
                Quantity = Convert.ToInt32(parameters["quantity"]),
                Date = DateTime.Parse(parameters["date"]?.ToString() ?? DateTime.MinValue.ToString()),
                Time = TimeSpan.Parse(parameters["time"]?.ToString() ?? "00:00:00")
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

        public Task<DialogflowResponse> HandleSearchTicket(Dictionary<string, object> parameters)
        {
            var result = new DialogflowResponse
            {
                FulfillmentText = $"Tìm thấy chuyến đi cho mã vé {parameters["ticket_number"]?.ToString()}"
            };

            return Task.FromResult(result);
        }

        private List<string> CheckMissingFields(Dictionary<string, object> parameters)
        {
            var requiredFields = new[] { "departure_station", "arrival_station", "date", "quantity", "time" };
            return requiredFields.Where(f => !parameters.ContainsKey(f) || parameters[f] == null).ToList();
        }
    }
}
