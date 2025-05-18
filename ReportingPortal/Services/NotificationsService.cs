using Models.http;
using System.Net.Http.Json;

namespace ReportingPortal.Services
{
    public class NotificationsService(HttpClient http)
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
    }
}
