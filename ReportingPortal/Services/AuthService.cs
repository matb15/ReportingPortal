using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Models.http;

namespace ReportingPortal.Services
{
    public class AuthService(HttpClient http)
    {
        private readonly HttpClient _http = http;

        public async Task<bool> RegisterAsync(string username, string password)
        {
            RegisterResponse response = (RegisterResponse) await _http.PostAsJsonAsync("api/Auth/register", new { username, password });
            return response.IsSuccessStatusCode;
        }

        public async Task<LoginResponse> LoginAsync(string username, string password)
        {
            LoginResponse response = (LoginResponse) await _http.PostAsJsonAsync("api/Auth/login", new { username, password });
            return response;
        }
    }
}