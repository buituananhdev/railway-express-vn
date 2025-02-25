using System.ComponentModel.DataAnnotations;
using Booking.Domain.Enums;
using Common.Domain;

namespace Booking.Domain.Entities;
public class PassengerInfo : BaseEntity
{
    public bool IsMainPassenger { get; set; }
    public AgeGroupEnum AgeGroup { get; set; }
    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; }
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; }
    [Required]
    [MaxLength(50)]
    public string LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    [Required]
    [MaxLength(30)]
    public string IdentityNumber { get; set; }
}
