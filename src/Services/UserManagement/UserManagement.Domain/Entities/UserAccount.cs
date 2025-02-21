using Common.Domain;

namespace UserManagement.Domain.Entities;
public class UserAccount : BaseEntity
{
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public RoleEnum Role { get; set; }
    public StatusEnum Status { get; set; }
}
