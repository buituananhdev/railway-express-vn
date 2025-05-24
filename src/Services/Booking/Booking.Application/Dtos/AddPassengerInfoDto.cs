using Booking.Domain.Enums;

namespace Booking.Application.Dtos;
public class AddPassengerInfoDto
{
    public bool IsMainPassenger { get; set; }
    public AgeGroupEnum AgeGroup { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? IdentityNumber { get; set; }
    public Guid TicketSeatId { get; set; }
}
