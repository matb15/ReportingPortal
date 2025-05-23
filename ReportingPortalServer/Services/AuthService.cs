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
    }

    public class AuthService : IAuthService
    {
        public LoginResponse LoginAsync(string username, string password, ApplicationDbContext context)
        {
            var user = context.Users.FirstOrDefault(u => u.Email == username && u.Password == password);
            if (user == null)
            {
                return new LoginResponse
                {
                    Message = "Credenziali non valide."
                };
            }

            var token = GenerateJwtToken(user);

            user.Password = "baldman"; 

            return new LoginResponse
            {
                User = user,
                Token = token,
                Message = "Login effettuato con successo."
            };
        }

        public RegisterResponse RegisterAsync(RegisterRequest request, ApplicationDbContext context)
        {
            var existingUser = context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return new RegisterResponse
                {
                    Message = "Email già in uso."
                };
            }
            var user = new User
            {

                Email = request.Email,
                Password = request.Password,
                Name = request.Name,
                Surname = request.Surname,
                Role = UserRoleEnum.User
            };
            context.Users.Add(user);
            context.SaveChanges();
            return new RegisterResponse
            {
                Message = "Registrazione avvenuta con successo."
            };
        }

        private string GenerateJwtToken(User user)
        {
            var key = new System.Text.UTF8Encoding().GetBytes("segreta"); 
            var claims = new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.Name),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.Role.ToString())
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
