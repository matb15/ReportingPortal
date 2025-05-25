using Microsoft.IdentityModel.Tokens;
using Models;
using Models.http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ReportingPortalServer.Services
{
    public interface IAuthService
    {
        public LoginResponse LoginAsync(string email, string password, ApplicationDbContext context);
        public RegisterResponse RegisterAsync(RegisterRequest request, ApplicationDbContext context);
    }

    public class AuthService : IAuthService
    {
        public LoginResponse LoginAsync(string email, string password, ApplicationDbContext context)
        {
            User? user = context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return new LoginResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Credenziali non valide."
                };
            }

            string token = GenerateJwtToken(user);

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
            User? existingUser = context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return new RegisterResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Conflict,
                    Message = "Email già in uso."
                };
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            User user = new()
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

        private static string GenerateJwtToken(User user)
        {
            byte[] key = new System.Text.UTF8Encoding().GetBytes("baldman_eroe_notturno_gey_che_combatte_contro_gli_etero");
            Claim[] claims =
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Surname, user.Surname),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
            ];

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            JwtSecurityTokenHandler tokenHandler = new();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
