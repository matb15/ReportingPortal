using Microsoft.EntityFrameworkCore;
using Models;
using Models.enums;
using Models.front;
using Models.http;
using ReportingPortalServer.Services.AppwriteIO;
using ReportingPortalServer.Services.Helpers;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace ReportingPortalServer.Services
{
    public interface IReportService
    {
        public ReportResponse GetReportById(string jwt, int id, ApplicationDbContext context);
        public Task<ReportsPaginatedResponse> GetPaginatedReports(string jwt, ReportsPaginatedRequest request, ApplicationDbContext context);
        public Task<ReportResponse> CreateReport(CreateReportRequest reportRequest, string jwt, ApplicationDbContext _context, IUploadFileService uploadFileService, IAppwriteClient appwriteClient);
        public ReportResponse DeleteReport(int idRep, string jwt, ApplicationDbContext _context);
        public ReportResponse UpdateReport(int idRep, CreateReportRequest updateRequest, string jwt, ApplicationDbContext _context);
        public Task<ClusterResponse> GetClusteredReports(/*string jwt,*/ ClusterRequest request, ApplicationDbContext context);
        public Task<ReportAnalyticsResponse> GetReportAnalytics(string jwt, bool IsPersonal, ApplicationDbContext context);
    }

    public class ReportService : IReportService
    {
        public ReportResponse GetReportById(string jwt, int id, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(jwt))
            {
                return new ReportResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }

            JwtSecurityToken token = handler.ReadJwtToken(jwt);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int parsedUserId))
            {
                return new ReportResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }

            Models.User? currentUser = context.Users.FirstOrDefault(u => u.Id == parsedUserId);
            if (currentUser == null)
            {
                return new ReportResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }

            Report? report = context.Reports
             .Include(r => r.User)
             .Include(r => r.Category)
             .Include(r => r.File)
             .FirstOrDefault(r => r.Id == id);

            if (report == null)
            {
                return new ReportResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Report not found."
                };
            }
            else
            {
                return new ReportResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Report retrieved successfully.",
                    Report = new ReportDto()
                    {
                        Id = report.Id,
                        Title = report.Title,
                        Description = report.Description,
                        CategoryId = report.CategoryId,
                        Location = report.Location,
                        LocationDetail = report.LocationDetail,
                        Latitude = report.GeoPoint.Y,
                        Longitude = report.GeoPoint.X,
                        UserId = report.UserId,
                        CreatedAt = report.CreatedAt,
                        Status = report.Status,
                        User = report.User,
                        Category = report.Category,
                        File = report.File,
                    }
                };
            }
        }
        public async Task<ReportsPaginatedResponse> GetPaginatedReports(string jwt, ReportsPaginatedRequest request, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(jwt))
            {
                return new ReportsPaginatedResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }
            JwtSecurityToken token = handler.ReadJwtToken(jwt);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int parsedUserId))
            {
                return new ReportsPaginatedResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }
            Models.User? currentUser = context.Users.FirstOrDefault(u => u.Id == parsedUserId);
            if (currentUser == null)
            {
                return new ReportsPaginatedResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }

            IQueryable<Report> query = context.Reports
             .Include(r => r.User)
             .Include(r => r.Category)
             .Include(r => r.File)
             .AsQueryable();

            if (request.IsPersonal && currentUser.Role == UserRoleEnum.Admin)
            {
                query = query.Where(r => r.UserId == currentUser.Id);
            }

            if (request.Status.HasValue)
            {
                query = query.Where(r => r.Status == request.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string search = request.Search.ToLower();
                query = query.Where(r =>
                    r.Title.ToLower().Contains(search) ||
                    r.Description.ToLower().Contains(search));
            }

            bool asc = request.SortAscending ?? true;

            if (!string.IsNullOrEmpty(request.SortField))
            {
                if (asc)
                {
                    query = query.OrderBy(u => EF.Property<object>(u, request.SortField));
                }
                else
                {
                    query = query.OrderByDescending(u => EF.Property<object>(u, request.SortField));
                }
            }
            else
            {
                query = query.OrderByDescending(u => u.CreatedAt);
            }

            int totalCount = await query.CountAsync();

            List<ReportDto> reports = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(report => new ReportDto
                {
                    Id = report.Id,
                    Title = report.Title,
                    Description = report.Description,
                    CategoryId = report.CategoryId,
                    Location = report.Location,
                    LocationDetail = report.LocationDetail,
                    Latitude = report.GeoPoint.Y,
                    Longitude = report.GeoPoint.X,
                    UserId = report.UserId,
                    CreatedAt = report.CreatedAt,
                    Status = report.Status,
                    User = report.User,
                    Category = report.Category,
                    File = report.File,
                })
                .ToListAsync();


            return new ReportsPaginatedResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Reports retrieved successfully.",
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                Items = reports
            };
        }
        public async Task<ReportResponse> CreateReport(CreateReportRequest reportRequest, string jwt, ApplicationDbContext _context, IUploadFileService uploadFileService, IAppwriteClient appwriteClient)
        {
            var culture = new CultureInfo("it-IT");

            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(jwt))
            {
                return new ReportResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }

            JwtSecurityToken token = handler.ReadJwtToken(jwt);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int parsedUserId))
            {
                return new ReportResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }

            Models.User? currentUser = _context.Users.FirstOrDefault(u => u.Id == parsedUserId);
            if (currentUser == null)
            {
                return new ReportResponse { StatusCode = (int)HttpStatusCode.BadRequest, Message = "Authenticated user not found." };
            }

            if (!double.TryParse(reportRequest.Latitude, NumberStyles.Float, culture, out double latitude))
            {
                return new ReportResponse
                {
                    StatusCode = 400,
                    Message = "Invalid latitude format."
                };
            }

            if (!double.TryParse(reportRequest.Longitude, NumberStyles.Float, culture, out double longitude))
            {
                return new ReportResponse
                {
                    StatusCode = 400,
                    Message = "Invalid longitude format."
                };
            }

            if (!int.TryParse(reportRequest.CategoryId.ToString(), out int categoryId) || categoryId <= 0)
            {
                return new ReportResponse
                {
                    StatusCode = 400,
                    Message = "Invalid category ID."
                };
            }

            int? fileId = null;

            var categoryExists = _context.Categories.Any(c => c.Id == categoryId);
            if (!categoryExists)
            {
                return new ReportResponse
                {
                    StatusCode = 400,
                    Message = $"Category with ID {reportRequest.CategoryId} does not exist."
                };
            }


            if (reportRequest.File != null)
            {
                var response = await uploadFileService.CreateUploadFile(reportRequest, _context, jwt, appwriteClient);
                if (response.StatusCode >= 200 && response.StatusCode < 300)
                {
                    fileId = response.File?.Id;
                }
                else
                {
                    return new ReportResponse
                    {
                        StatusCode = response.StatusCode,
                        Message = response.Message
                    };
                }
            }

            Report report = new()
            {
                Title = reportRequest.Title,
                Description = reportRequest.Description,
                CategoryId = categoryId,
                Location = reportRequest.Location,
                LocationDetail = reportRequest.LocationDetail ?? "",
                GeoPoint = new NetTopologySuite.Geometries.Point(longitude, latitude)
                {
                    SRID = 4326
                },
                UserId = currentUser.Id,
                CreatedAt = DateTime.UtcNow,
                FileId = fileId,
            };

            try
            {
                _context.Reports.Add(report);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "No inner exception details";

                return new ReportResponse
                {
                    StatusCode = 500,
                    Message = $"An error occurred while processing your request. {ex.Message} Inner exception: {innerMessage}"
                };
            }



            return new ReportResponse
            {
                StatusCode = (int)HttpStatusCode.Created,
                Message = "Report created successfully.",
                Report = new ReportDto()
                {
                    Id = report.Id,
                    Title = report.Title,
                    Description = report.Description,
                    CategoryId = report.CategoryId,
                    Location = report.Location,
                    LocationDetail = report.LocationDetail,
                    Latitude = report.GeoPoint.Y,
                    Longitude = report.GeoPoint.X,
                    UserId = report.UserId,
                    CreatedAt = report.CreatedAt,
                    Status = report.Status,
                    User = report.User,
                    Category = report.Category,
                    File = report.File,
                }
            };
        }
        public ReportResponse DeleteReport(int idRep, string jwt, ApplicationDbContext _context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(jwt))
            {
                return new ReportResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }
            JwtSecurityToken token = handler.ReadJwtToken(jwt);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int parsedUserId))
            {
                return new ReportResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }
            Models.User? currentUser = _context.Users.FirstOrDefault(u => u.Id == parsedUserId);
            if (currentUser == null)
            {
                return new ReportResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }

            Report? report = null;

            if (currentUser.Role == UserRoleEnum.Admin)
            {
                // L'admin può cancellare qualsiasi report
                report = _context.Reports.FirstOrDefault(r => r.Id == idRep);


            }
            else
            {
                // Un utente normale può cancellare solo i suoi
                report = _context.Reports.FirstOrDefault(r => r.Id == idRep && r.UserId == currentUser.Id);
            }

            if (report == null)
            {
                return new ReportResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Report not found or you do not have permission to delete it."
                };
            }

            _context.Reports.Remove(report);
            _context.SaveChanges();
            return new ReportResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Report deleted successfully.",
            };
        }
        public ReportResponse UpdateReport(int idRep, CreateReportRequest updateRequest, string jwt, ApplicationDbContext _context)
        {
            var culture = new CultureInfo("it-IT");

            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(jwt))
            {
                return new ReportResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }
            JwtSecurityToken token = handler.ReadJwtToken(jwt);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int parsedUserId))
            {
                return new ReportResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }
            Models.User? currentUser = _context.Users.FirstOrDefault(u => u.Id == parsedUserId);
            if (currentUser == null)
            {
                return new ReportResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }
            if (currentUser.Role != UserRoleEnum.Admin)
            {
                return new ReportResponse
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Message = "Only admin can update reports."
                };
            }
            Report? report = _context.Reports.FirstOrDefault(r => r.Id == idRep);
            if (report == null)
            {
                return new ReportResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Report not found."
                };
            }

            if (!double.TryParse(updateRequest.Latitude, NumberStyles.Float, culture, out double latitude))
            {
                return new ReportResponse
                {
                    StatusCode = 400,
                    Message = "Invalid latitude format."
                };
            }

            if (!double.TryParse(updateRequest.Longitude, NumberStyles.Float, culture, out double longitude))
            {
                return new ReportResponse
                {
                    StatusCode = 400,
                    Message = "Invalid longitude format."
                };
            }

            if (!int.TryParse(updateRequest.CategoryId.ToString(), out int categoryId) || categoryId <= 0)
            {
                return new ReportResponse
                {
                    StatusCode = 400,
                    Message = "Invalid category ID."
                };
            }

            int? fileId = null;

            var categoryExists = _context.Categories.Any(c => c.Id == categoryId);
            if (!categoryExists)
            {
                return new ReportResponse
                {
                    StatusCode = 400,
                    Message = $"Category with ID {updateRequest.CategoryId} does not exist."
                };
            }

            report.Title = updateRequest.Title;
            report.Description = updateRequest.Description;
            report.CategoryId = categoryId;
            report.Location = updateRequest.Location;
            report.LocationDetail = updateRequest.LocationDetail ?? "";
            report.GeoPoint = new NetTopologySuite.Geometries.Point(longitude, latitude) { SRID = 4326 };

            _context.Reports.Update(report);

            _context.SaveChanges();

            return new ReportResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Report updated successfully.",
                Report = new ReportDto()
                {
                    Id = report.Id,
                    Title = report.Title,
                    Description = report.Description,
                    CategoryId = report.CategoryId,
                    Location = report.Location,
                    LocationDetail = report.LocationDetail,
                    Latitude = report.GeoPoint.Y,
                    Longitude = report.GeoPoint.X,
                    UserId = report.UserId,
                    CreatedAt = report.CreatedAt,
                    Status = report.Status,
                    User = report.User,
                    Category = report.Category,
                    File = report.File,
                }
            };
        }

        public async Task<ClusterResponse> GetClusteredReports(ClusterRequest request, ApplicationDbContext context)
        {
            // Optional: JWT parsing logic (left commented out)
            /*
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(jwt))
            {
                return new ClusterResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }

            JwtSecurityToken token = handler.ReadJwtToken(jwt);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int parsedUserId))
            {
                return new ClusterResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }

            Models.User? currentUser = await context.Users.FindAsync(parsedUserId);
            if (currentUser == null)
            {
                return new ClusterResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }
            */

            var query = context.Reports
                .Include(r => r.User)
                .Include(r => r.Category)
                .AsQueryable();

            if (request.Status.HasValue)
            {
                query = query.Where(r => r.Status == request.Status.Value);
            }

            if (request.CategoryId.HasValue)
            {
                query = query.Where(r => r.CategoryId == request.CategoryId.Value);
            }

            bool hasAny = await query.AnyAsync();
            if (!hasAny)
            {
                return new ClusterResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "No reports found."
                };
            }

            List<Report> reports = await query.ToListAsync();

            List<ReportCluster> clusters = Utils.ClusterReports(reports, request.Zoom);

            return new ClusterResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Clustered reports retrieved successfully.",
                Clusters = clusters.Select(c => new Cluster<ReportDto>
                {
                    Items = c.Reports?.Select(r => new ReportDto
                    {
                        Id = r.Id,
                        Title = r.Title,
                        Status = r.Status,
                        Longitude = r.Longitude,
                        Latitude = r.Latitude
                    }).ToList() ?? []
                }).ToList()
            };
        }

        public async Task<ReportAnalyticsResponse> GetReportAnalytics(string jwt, bool IsPersonal, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(jwt))
            {
                return new ReportAnalyticsResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }
            JwtSecurityToken token = handler.ReadJwtToken(jwt);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int parsedUserId))
            {
                return new ReportAnalyticsResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }
            Models.User? currentUser = context.Users.FirstOrDefault(u => u.Id == parsedUserId);
            if (currentUser == null)
            {
                return new ReportAnalyticsResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }
            IQueryable<Report> query = context.Reports.AsQueryable();

            if (currentUser.Role != UserRoleEnum.Admin)
            {
                query = query.Where(r => r.UserId == currentUser.Id);
            }

            if (IsPersonal && currentUser.Role == UserRoleEnum.Admin)
            {
                query = query.Where(r => r.UserId == currentUser.Id);
            }

            var totalReports = await query.CountAsync();

            var pendingReports = await query
                .Where(r => r.Status == ReportStatusEnum.Pending)
                .CountAsync();

            var resolvedReports = await query
                .Where(r => r.Status == ReportStatusEnum.Resolved)
                .CountAsync();

            var rejectedReports = await query
                .Where(r => r.Status == ReportStatusEnum.Rejected)
                .CountAsync();

            var dailyReportCounts = await query
                .GroupBy(r => r.CreatedAt.Date)
                .Select(g => new DailyReportCount
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(d => d.Date)
                .ToListAsync();

            return new ReportAnalyticsResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Report analytics retrieved successfully.",
                TotalReports = totalReports,
                PendingReports = pendingReports,
                ResolvedReports = resolvedReports,
                RejectedReports = rejectedReports,
                DailyReportCounts = dailyReportCounts
            };

        }
    }
}
