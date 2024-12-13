using Common.Domain;

namespace Passenger.Domain.Entities
{
    public class Passenger : BaseEntity
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public string? Address { get; set; }
    }
}
