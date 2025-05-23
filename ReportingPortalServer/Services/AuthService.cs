using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Threading.Tasks;
using Models;
using Models.http;
using Models.enums;

namespace ReportingPortalServer.Services
{
    public interface IAuthService
    {
        public LoginResponse LoginAsync(string username, string password, ApplicationDbContext context);
        public RegisterResponse RegisterAsync(RegisterRequest request, ApplicationDbContext context);
        public User GetMeAsync(string JWT, ApplicationDbContext context);
    }

    public class AuthService : IAuthService
    {
        public LoginResponse LoginAsync(string username, string password, ApplicationDbContext context)
        {
            var user = context.Users.FirstOrDefault(u => u.Email == username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return new LoginResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Credenziali non valide."
                };
            }

            var token = GenerateJwtToken(user);

            user.Password = "baldman";

            return new LoginResponse
            {
                User = user,
                Token = token,
                Message = "Login effettuato con successo.",
                StatusCode = (int)System.Net.HttpStatusCode.OK
            };
        }



        public RegisterResponse RegisterAsync(RegisterRequest request, ApplicationDbContext context)
        {
            var existingUser = context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return new RegisterResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Conflict,
                    Message = "Email già in uso."
                };
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Email = request.Email,
                Password = hashedPassword,
                Name = request.Name,
                Surname = request.Surname,
                Role = 0
            };

            context.Users.Add(user);
            context.SaveChanges();

            return new RegisterResponse
            {
                StatusCode = (int)System.Net.HttpStatusCode.Created,
                Message = "Registrazione avvenuta con successo."
            };
        }



        public User GetMeAsync(string JWT, ApplicationDbContext context)
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();

            if (!handler.CanReadToken(JWT))
                return null;

            var token = handler.ReadJwtToken(JWT);

            var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null)
                return null;

            if (!int.TryParse(userIdClaim.Value, out var userId))
                return null;

            var user = context.Users.FirstOrDefault(u => u.Id == userId);
            user.Password = "baldman";
            return user;
        }



        private string GenerateJwtToken(User user)
        {
            var key = new System.Text.UTF8Encoding().GetBytes("baldman_eroe_notturno_gey_che_combatte_contro_gli_etero"); 
            var claims = new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.Name),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Surname, user.Surname),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email)
            };

            var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                    new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                    Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
