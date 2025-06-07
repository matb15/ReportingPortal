using Models.http;
using System.Net.Http.Json;

namespace ReportingPortal.Services
{
    public class NotificationService(HttpClient http)
    {
        private readonly HttpClient _http = http;

        public async Task<NotificationsPaginatedResponse> GetAllAsync(NotificationsPaginatedRequest request)
        {
            string url = $"api/Notification?page={request.Page}&pageSize={request.PageSize}";
            try
            {
                HttpResponseMessage response = await _http.GetAsync(url);
                NotificationsPaginatedResponse? content = await response.Content.ReadFromJsonAsync<NotificationsPaginatedResponse>();
                if (content != null && response.IsSuccessStatusCode)
                {
                    return content;
                }
                else
                {
                    return new NotificationsPaginatedResponse
                    {
                        Message = content?.Message ?? "Failed to fetch notifications.",
                        StatusCode = (int)response.StatusCode,
                        Page = request.Page,
                        PageSize = request.PageSize
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch notifications: {ex.Message}");
                return new NotificationsPaginatedResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500,
                    Page = request.Page,
                    PageSize = request.PageSize
                };
            }
        }

        public async Task<NotificationConnectResponse> SubscribeToPushNotificationsAsync(string client, string endpoint, string p256dh, string auth)
        {
            string url = "api/Notification/connect-push-notifications";

            NotificationConnectRequest subscription = new()
            {
                Client = client,
                Endpoint = endpoint,
                P256dh = p256dh,
                Auth = auth
            };

            try
            {
                HttpResponseMessage response = await _http.PostAsJsonAsync(url, subscription);
                NotificationConnectResponse? content = await response.Content.ReadFromJsonAsync<NotificationConnectResponse>();
                if (content != null && response.IsSuccessStatusCode)
                {
                    return content;
                }
                else
                {
                    return new NotificationConnectResponse
                    {
                        Message = content?.Message ?? "Failed to subscribe to push notifications.",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                return new NotificationConnectResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<NotificationResponse> DeleteAsync(int notificationId)
        {
            string url = $"api/Notification/{notificationId}";
            try
            {
                HttpResponseMessage response = await _http.DeleteAsync(url);
                NotificationResponse? content = await response.Content.ReadFromJsonAsync<NotificationResponse>();
                if (content != null && response.IsSuccessStatusCode)
                {
                    return content;
                }
                else
                {
                    return new NotificationResponse
                    {
                        Message = content?.Message ?? "Failed to delete notification.",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                return new NotificationResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }
    }
}
