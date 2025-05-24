using Booking.Domain.Enums;
using Common.Domain;

namespace Booking.Domain.Entities;
public class PassengerInfo : BaseEntity
{
    public bool IsMainPassenger { get; set; }
    public AgeGroupEnum AgeGroup { get; set; }
    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? IdentityNumber { get; set; }
    public Guid TicketSeatId { get; set; }
    public TicketSeat TicketSeat { get; set; }
}
