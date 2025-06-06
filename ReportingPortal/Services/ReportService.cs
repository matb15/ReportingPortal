using Models.http;
using System.Net.Http.Json;

namespace ReportingPortal.Services
{
    public class ReportService(HttpClient http)
    {
        private readonly HttpClient _http = http;

        public async Task<ReportsPaginatedResponse> GetAllAsync(ReportsPaginatedRequest request)
        {
            string url = $"api/reports/paginated?page={request.Page}&pageSize={request.PageSize}";
            try
            {
                HttpResponseMessage response = await _http.GetAsync(url);
                ReportsPaginatedResponse? content = await response.Content.ReadFromJsonAsync<ReportsPaginatedResponse>();
                if (content != null && response.IsSuccessStatusCode)
                {
                    return content;
                }
                else
                {
                    return new ReportsPaginatedResponse
                    {
                        Message = content?.Message ?? "Failed to fetch reports.",
                        StatusCode = (int)response.StatusCode,
                        Page = request.Page,
                        PageSize = request.PageSize
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch reports: {ex.Message}");
                return new ReportsPaginatedResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500,
                    Page = request.Page,
                    PageSize = request.PageSize
                };
            }
        }
    }
}
