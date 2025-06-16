using Models.http;
using System.Net.Http.Json;

namespace ReportingPortal.Services
{
    public class ReportService(IHttpClientFactory factory)
    {
        private readonly HttpClient _http = factory.CreateClient("AuthorizedClient");

        public async Task<ReportsPaginatedResponse> GetAllAsync(ReportsPaginatedRequest request)
        {
            List<string> queryParams =
            [
                $"page={request.Page}",
                $"pageSize={request.PageSize}"
            ];

            if (!string.IsNullOrWhiteSpace(request.Search))
                queryParams.Add($"search={Uri.EscapeDataString(request.Search)}");

            if (!string.IsNullOrWhiteSpace(request.SortField))
                queryParams.Add($"sortField={Uri.EscapeDataString(request.SortField)}");

            if (request.SortAscending.HasValue)
                queryParams.Add($"sortAscending={request.SortAscending.Value.ToString().ToLower()}");

            if (request.Status.HasValue)
                queryParams.Add($"status={(int)request.Status.Value}");

            string url = $"api/report?{string.Join("&", queryParams)}";

            try
            {
                HttpResponseMessage response = await _http.GetAsync(url);
                ReportsPaginatedResponse? content = await response.Content.ReadFromJsonAsync<ReportsPaginatedResponse>();

                return content != null && response.IsSuccessStatusCode
                    ? content
                    : new ReportsPaginatedResponse
                    {
                        Message = content?.Message ?? "Failed to fetch reports.",
                        StatusCode = (int)response.StatusCode,
                        Page = request.Page,
                        PageSize = request.PageSize
                    };
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


        public async Task<ReportResponse> GetByIdAsync(int id)
        {
            string url = $"api/report/{id}";
            try
            {
                HttpResponseMessage response = await _http.GetAsync(url);
                ReportResponse? content = await response.Content.ReadFromJsonAsync<ReportResponse>();
                if (content != null && response.IsSuccessStatusCode)
                {
                    return content;
                }
                else
                {
                    return new ReportResponse
                    {
                        Message = content?.Message ?? "Failed to fetch report.",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch report: {ex.Message}");
                return new ReportResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<ReportResponse> CreateAsync(MultipartFormDataContent model)
        {
            string url = "api/report";
            try
            {
                HttpResponseMessage response = await _http.PostAsync(url, model);
                ReportResponse? content = await response.Content.ReadFromJsonAsync<ReportResponse>();
                if (content != null && response.IsSuccessStatusCode)
                {
                    return content;
                }
                else
                {
                    return new ReportResponse
                    {
                        Message = content?.Message ?? "Failed to create report.",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create report: {ex.Message}");
                return new ReportResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<ReportResponse> UpdateAsync(CreateReportRequest model)
        {
            string url = $"api/report/{model.Id}";
            try
            {
                HttpResponseMessage response = await _http.PutAsJsonAsync(url, model);
                ReportResponse? content = await response.Content.ReadFromJsonAsync<ReportResponse>();
                if (content != null && response.IsSuccessStatusCode)
                {
                    return content;
                }
                else
                {
                    return new ReportResponse
                    {
                        Message = content?.Message ?? "Failed to update report.",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to update report: {ex.Message}");
                return new ReportResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<ReportResponse> DeleteAsync(int id)
        {
            string url = $"api/report/{id}";
            try
            {
                HttpResponseMessage response = await _http.DeleteAsync(url);
                ReportResponse? content = await response.Content.ReadFromJsonAsync<ReportResponse>();
                if (content != null && response.IsSuccessStatusCode)
                {
                    return content;
                }
                else
                {
                    return new ReportResponse
                    {
                        Message = content?.Message ?? "Failed to update report.",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete report: {ex.Message}");
                return new ReportResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }
    }
}
