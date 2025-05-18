using Models.http;
using System.Net.Http.Json;

namespace ReportingPortal.Services
{
    public class ReportsService(HttpClient http)
    {
        private readonly HttpClient _http = http;

        public async Task<ReportsPaginatedResponse?> GetPaginatedReportsAsync(int page = 1, int pageSize = 5)
        {
            var url = $"api/reports/paginated?page={page}&pageSize={pageSize}";
            try
            {
                var response = await _http.GetFromJsonAsync<ReportsPaginatedResponse>(url);
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch reports: {ex.Message}");
                return null;
            }
        }
    }
}
