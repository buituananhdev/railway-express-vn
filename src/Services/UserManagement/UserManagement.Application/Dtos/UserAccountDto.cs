using Common.Application.Dtos;
using Common.Domain;

namespace UserManagement.Application.Dtos;
public class UserAccountDto : BaseDto
{
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public RoleEnum Role { get; set; }
    public StatusEnum Status { get; set; } = StatusEnum.Active;
}
