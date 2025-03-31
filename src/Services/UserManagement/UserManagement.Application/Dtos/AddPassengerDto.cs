namespace UserManagement.Application.Dtos;
public class AddPassengerDto
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string Role { get; set; }
    public bool Active { get; set; }
}
