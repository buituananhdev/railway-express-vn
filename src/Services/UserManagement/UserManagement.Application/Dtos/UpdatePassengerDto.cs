namespace UserManagement.Application.Dtos;
public class UpdatePassengerDto
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string NewPassword { get; set; }
    public string PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string Role { get; set; }
}
