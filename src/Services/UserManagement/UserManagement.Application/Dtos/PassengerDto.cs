using Common.Application.Dtos;
using Common.Domain;

namespace UserManagement.Application.Dtos;
public class PassengerDto : BaseDto
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string? Address { get; set; }
    public RoleEnum Role { get; set; }
}
