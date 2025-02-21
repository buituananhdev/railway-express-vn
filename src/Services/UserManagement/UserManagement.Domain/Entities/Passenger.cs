using Common.Domain;
using UserManagement.Domain.Entities;

namespace UserManagement.Domain.Entities
{
    public class Passenger : BaseEntity
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public string? Address { get; set; }
        public Guid UserAccountId { get; set; }
        public UserAccount UserAccount { get; set; }
    }
}
