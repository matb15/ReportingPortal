using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Models.http;

namespace ReportingPortal.Services
{
    public class AuthService(HttpClient http)
    {
        private readonly HttpClient _http = http;




        public async Task<RegisterResponse> RegisterAsync(RegisterRequest registerRequest)
        {
            var response = await _http.PostAsJsonAsync("api/Auth/register", registerRequest);
            if (response.IsSuccessStatusCode)
            {
                var registerResponse = await response.Content.ReadFromJsonAsync<RegisterResponse>();
                return registerResponse;
            }
            else
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<RegisterResponse>();
                return new RegisterResponse
                {
                    Message = errorResponse.Message,
                    StatusCode = (int)errorResponse.StatusCode
                };
            }
        }



        public async Task<LoginResponse> LoginAsync(string username, string password)
        {
            var response = await _http.PostAsJsonAsync("api/Auth/login", new { username, password });
            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                return loginResponse;
            }
            else
            { 
                var errorResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                return new LoginResponse
                {
                    Message = errorResponse.Message,
                    StatusCode = (int)errorResponse.StatusCode
                };
            }
        }



    }
}