namespace Booking.Application.Dtos;
public class AddPassengerDetailsDto
{
    public Guid TicketId { get; set; }
    public List<AddPassengerInfoDto> PassengerInfos { get; set; }
}
