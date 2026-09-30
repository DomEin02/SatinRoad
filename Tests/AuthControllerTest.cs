using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using API.Dtos;
using Infa;
using LinqToDB;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Tests;

public class AuthControllerTests
{
    public class RegisterTests : ApiTest
    {
        [Fact]
        public void Creates_a_normal_user()
        {
            var result = AuthController.Register(new RegisterRequest("Larry", "password123"));
            
            Assert.Equal("User", result.Role);
            Assert.Single(Db.Users().ToList());
        }

        [Fact]
        public void Stores_a_hash_not_the_password()
        {
            AuthController.Register(new RegisterRequest("Larry", "password123"));

            var row = Db.Users().Single();

            // The real password must never be saved...
            Assert.NotEqual("password123", row.PasswordHash);
            // ...but the saved hash must still match it
            Assert.True(BCrypt.Net.BCrypt.Verify("password123", row.PasswordHash));
        }

        [Fact]
        public void Rejects_a_short_password()
        {
            Assert.Throws<ValidationException>(() =>
                AuthController.Register(new RegisterRequest("Larry", "short")));
        }

        [Fact]
        public void Rejects_a_taken_username_even_with_different_capitals()
        {
            AuthController.Register(new RegisterRequest("Larry", "password123"));

            // "larry" and "Larry" count as the same name
            Assert.Throws<InvalidOperationException>(() =>
                AuthController.Register(new RegisterRequest("larry", "password123")));
        }
    }

    public class LoginTests : ApiTest
    {
        [Fact]
        public void Returns_a_token_for_correct_details()
        {
            AuthController.Register(new RegisterRequest("Larry", "password123"));

            var result = AuthController.Login(new LoginRequest("Larry", "password123"));

            Assert.False(string.IsNullOrWhiteSpace(result.Token));
            Assert.Equal("Larry", result.User.Username);
        }

        [Fact]
        public void Puts_the_admin_role_in_an_admins_token()
        {
            // Register can't create admins, so we insert one directly into the database
            Db.Insert(new User
            {
                Id = "a1",
                Username = "boss",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Role = UserRole.Admin,
                CreatedAtUtc = DateTime.UtcNow
            });

            var result = AuthController.Login(new LoginRequest("boss", "password123"));

            // Read the token back and check which role is written inside it
            var token = new JsonWebToken(result.Token);
            Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        }

        [Fact]
        public void Rejects_a_wrong_password()
        {
            AuthController.Register(new RegisterRequest("Larry", "password123"));

            Assert.Throws<UnauthorizedAccessException>(() =>
                AuthController.Login(new LoginRequest("Larry", "wrongpassword")));
        }

        [Fact]
        public void Rejects_an_unknown_username()
        {
            Assert.Throws<UnauthorizedAccessException>(() =>
                AuthController.Login(new LoginRequest("nobody", "password123")));
        }

        [Fact]
        public void Rejects_a_shut_down_user()
        {
            // Arrange: a user who has been shut down by the FBI
            Db.Insert(new User
            {
                Id = "u1",
                Username = "raided",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Role = UserRole.User,
                IsShutDown = true,
                CreatedAtUtc = DateTime.UtcNow
            });

            // Act + Assert: even the correct password is refused
            Assert.Throws<UnauthorizedAccessException>(() =>
                AuthController.Login(new LoginRequest("raided", "password123")));
        }
    }
}