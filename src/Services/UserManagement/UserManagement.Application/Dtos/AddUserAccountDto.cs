using Common.Domain;

namespace UserManagement.Application.Dtos;
public class AddUserAccountDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    public RoleEnum Role { get; set; }
    public StatusEnum Status { get; set; } = StatusEnum.Active;
}
