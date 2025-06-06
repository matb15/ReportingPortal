using Models;
using Models.http;
using System.Net.Http.Json;

namespace ReportingPortal.Services
{
    public class CategoryService(HttpClient http)
    {
        private readonly HttpClient _http = http;

        public async Task<CategoriesPaginatedResponse> GetAllAsync(CategoriesPaginatedRequest request)
        {
            string url = $"api/Category?page={request.Page}&pageSize={request.PageSize}";
            try
            {
                HttpResponseMessage response = await _http.GetAsync(url);
                CategoriesPaginatedResponse? content = await response.Content.ReadFromJsonAsync<CategoriesPaginatedResponse>();
                if (content != null && response.IsSuccessStatusCode)
                {
                    return content;
                }
                else
                {
                    return new CategoriesPaginatedResponse
                    {
                        Message = content?.Message ?? "Failed to fetch categories.",
                        StatusCode = (int)response.StatusCode,
                        Page = request.Page,
                        PageSize = request.PageSize
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch categories: {ex.Message}");
                return new CategoriesPaginatedResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500,
                    Page = request.Page,
                    PageSize = request.PageSize
                };
            }
        }

        public async Task<CategoryResponse> CreateAsync(Category request)
        {
            string url = "api/Category";
            try
            {
                HttpResponseMessage response = await _http.PostAsJsonAsync(url, request);
                CategoryResponse? content = await response.Content.ReadFromJsonAsync<CategoryResponse>();
                if (content != null && response.IsSuccessStatusCode)
                {
                    return content;
                }
                else
                {
                    return new CategoryResponse
                    {
                        Message = content?.Message ?? "Failed to create category.",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                return new CategoryResponse
                {
                    Message = $"Request failed: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<Response> DeleteAsync(int id)
        {
            string url = $"api/Category/{id}";
            try
            {
                HttpResponseMessage response = await _http.DeleteAsync(url);
                Response? content = await response.Content.ReadFromJsonAsync<Response>();
                if (content != null && response.IsSuccessStatusCode)
                {
                    return content;
                }
                else
                {
                    return new Response
                    {
                        Message = content?.Message ?? "Failed to delete category.",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
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
