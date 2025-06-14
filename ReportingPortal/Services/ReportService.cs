using Models;
using Models.http;
using System.Net.Http.Json;

namespace ReportingPortal.Services
{
    public class ReportService(IHttpClientFactory factory)
    {
        private readonly HttpClient _http = factory.CreateClient("AuthorizedClient");

        public async Task<ReportsPaginatedResponse> GetAllAsync(ReportsPaginatedRequest request)
        {
            string url = $"api/report?page={request.Page}&pageSize={request.PageSize}";
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

        public async Task<ReportResponse> CreateAsync(Report model)
        {
            string url = "api/report";
            try
            {
                HttpResponseMessage response = await _http.PostAsJsonAsync(url, model);
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

        public async Task<ReportResponse> UpdateAsync(int id, Report model)
        {
            string url = $"api/report/{id}";
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

        public async Task<Response> DeleteAsync(int id)
        {
            string url = $"api/report/{id}";
            try
            {
                HttpResponseMessage response = await _http.DeleteAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        Message = "Report deleted successfully.",
                        StatusCode = (int)response.StatusCode
                    };
                }
                else
                {
                    return new Response
                    {
                        Message = "Failed to delete report.",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete report: {ex.Message}");
                return new Response
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }
    }
}
