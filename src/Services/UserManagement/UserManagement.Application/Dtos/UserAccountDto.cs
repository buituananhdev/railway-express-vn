using Common.Domain;

namespace UserManagement.Application.Dtos;
public class UserAccountDto
{
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public RoleEnum Role { get; set; }
    public StatusEnum Status { get; set; } = StatusEnum.Active;
}
