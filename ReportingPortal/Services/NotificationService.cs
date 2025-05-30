using Models.http;
using System.Net.Http.Json;

namespace ReportingPortal.Services
{
    public class NotificationService(HttpClient http)
    {
        private readonly HttpClient _http = http;

        public async Task<NotificationsPaginatedResponse?> GetPaginatedNotificationsAsync(int page = 1, int pageSize = 5)
        {
            var url = $"api/notifications/paginated?page={page}&pageSize={pageSize}";
            try
            {
                var response = await _http.GetFromJsonAsync<NotificationsPaginatedResponse>(url);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch notifications: {ex.Message}");
                return null;
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
    }
}
