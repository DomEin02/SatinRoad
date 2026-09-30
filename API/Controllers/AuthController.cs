using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using API.Dtos;
using Infa;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers;

[Route("[controller]")]
public class AuthController(SatinRoadDatabase db, IConfiguration config) : ControllerBase
{
    [HttpPost(nameof(Register))]
    public UserResponse Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            throw new ValidationException("Username is required");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            throw new ValidationException("Password must be at least 8 characters");

        var username = request.Username.Trim();

        if (db.Users().Any(u => u.Username.ToLower() == username.ToLower()))
            throw new InvalidOperationException("Username is already taken");
        
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.User,
            IsShutDown = false,
            CreatedAtUtc = DateTime.UtcNow
        };
        
        db.Insert(user);
        
        return new UserResponse(user);
    }
    
    [HttpPost(nameof(Login))]
    public LoginResponse Login([FromBody] LoginRequest request)
    {
        var username = (request.Username ?? "").Trim();
        var user = db.Users().FirstOrDefault(u => u.Username.ToLower() == username.ToLower());
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password ?? "", user.PasswordHash))
            throw new UnauthorizedAccessException("Wrong username or password");
        
        if (user.IsShutDown)
            throw new UnauthorizedAccessException("This account has been shut down");
        
        return new LoginResponse(CreateToken(user), user);
    }

    private string CreateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = config["Jwt:Issuer"],
            Audience = config["Jwt:Issuer"],
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = credentials
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}