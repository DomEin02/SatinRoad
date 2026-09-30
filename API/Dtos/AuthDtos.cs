namespace API.Dtos;

public record RegisterRequest(string Username, string Password);

public record LoginRequest(string Username, string Password);

public class UserResponse
{
    public string Id { get; }
    public string Username { get; }
    public string Role { get; }

    public UserResponse(Infa.User user)
    {
        Id = user.Id;
        Username = user.Username;
        Role = user.Role.ToString();
    }
}

public class LoginResponse
{
    public string Token { get; }
    public UserResponse User { get; }

    public LoginResponse(string token, Infa.User user)
    {
        Token = token;
        User = new UserResponse(user);
    }
}