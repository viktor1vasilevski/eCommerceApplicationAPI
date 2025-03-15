namespace Main.DTOs.Auth;

public class LoginDTO
{
    public Guid Id { get; set; }
    public string Token { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
}
