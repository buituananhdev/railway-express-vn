using System.ComponentModel.DataAnnotations;
using Booking.Domain.Enums;
using Common.Domain;

namespace Booking.Domain.Entities;
public class PassengerInfo : BaseEntity
{
    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; }
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; }
    [Required]
    [MaxLength(50)]
    public string LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    [Required]
    [Phone]
    public string PhoneNumber { get; set; }
    [Required]
    [MaxLength(30)]
    public string IdentityNumber { get; set; }
    public IdentityTypeEnum IdentityType { get; set; }
}
