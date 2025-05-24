using Models.front;
using Models.http;
using System.Net.Http.Json;

namespace ReportingPortal.Services
{
    public class UserService(IHttpClientFactory factory)
    {
        private readonly HttpClient _http = factory.CreateClient("AuthorizedClient");

        public async Task<UserResponse> GetMeAsync()
        {
            try
            {
                HttpResponseMessage response = await _http.GetAsync("api/User/me");
                UserResponse? content = await response.Content.ReadFromJsonAsync<UserResponse>();

                if (response.IsSuccessStatusCode && content != null)
                {
                    return content;
                }

                return new UserResponse
                {
                    Message = content?.Message ?? "An unknown error occurred.",
                    StatusCode = content?.StatusCode ?? (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new UserResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<UserResponse> UpdateMeAsync(UserPutModel updatedUser)
        {
            try
            {
                HttpResponseMessage response = await _http.PutAsJsonAsync("api/User/me", updatedUser);
                UserResponse? content = await response.Content.ReadFromJsonAsync<UserResponse>();

                if (response.IsSuccessStatusCode && content != null)
                {
                    return content;
                }

                return new UserResponse
                {
                    Message = content?.Message ?? "Error updating user",
                    StatusCode = content?.StatusCode ?? (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new UserResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<Response> UpdateMePasswordAsync(string oldPassword, string newPassword)
        {
            try
            {
                ChangePasswordFormModel payload = new()
                {
                    CurrentPassword = oldPassword,
                    NewPassword = newPassword,
                    ConfirmPassword = newPassword
                };

                HttpResponseMessage response = await _http.PutAsJsonAsync("api/User/me/password", payload);
                Response? content = await response.Content.ReadFromJsonAsync<Response>();

                if (response.IsSuccessStatusCode && content != null)
                    return content;

                return new Response
                {
                    Message = content?.Message ?? "Error updating password",
                    StatusCode = content?.StatusCode ?? (int)response.StatusCode
                };
            }
            catch (HttpRequestException ex)
            {
                return new Response
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<Response> DeleteMeAsync()
        {
            try
            {
                HttpResponseMessage response = await _http.DeleteAsync("api/User/me");
                Response? content = await response.Content.ReadFromJsonAsync<Response>();

                if (response.IsSuccessStatusCode && content != null)
                    return content;

                return new Response
                {
                    Message = content?.Message ?? "Error deleting account",
                    StatusCode = content?.StatusCode ?? (int)response.StatusCode
                };
            }
            catch (HttpRequestException ex)
            {
                return new Response
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }
    }
}
