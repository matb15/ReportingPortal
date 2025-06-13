using Appwrite.Models;
using Appwrite;
using Models;
using Models.http;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Models.enums;

namespace ReportingPortalServer.Services
{
    public interface IReportService
    {
        public ReportResponse GetReportById(string jwt, int id, ApplicationDbContext context);
        public ReportsPaginatedResponse GetPaginatedReports(string jwt, int page, int pageSize, ApplicationDbContext context);
        public ReportResponse CreateReport(ReportRequest reportRequest, string jwt, ApplicationDbContext _context);
        public ReportResponse DeleteReport(int idRep, int idUser, string jwt, ApplicationDbContext _context);
        public ReportResponse UpdateReport(int idRep, ReportRequest updateRequest, string jwt, ApplicationDbContext _context);
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

            Report? report = context.Reports.FirstOrDefault(r => r.Id == id);
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
                    Report = report
                };
            }
        }
        public ReportsPaginatedResponse GetPaginatedReports(string jwt, int page, int pageSize, ApplicationDbContext context)
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

            List<Report> reports = [..context.Reports
                .Skip(page * pageSize)
                .Take(pageSize)];

            int totalCount = context.Reports.Count();
            return new ReportsPaginatedResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Reports retrieved successfully.",
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = reports
            };
        }
        public ReportResponse CreateReport(ReportRequest reportRequest, string jwt, ApplicationDbContext _context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(jwt))
            {
                throw new ArgumentException("JWT not valid.");
            }
            JwtSecurityToken token = handler.ReadJwtToken(jwt);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int parsedUserId))
            {
                throw new ArgumentException("JWT does not contain user ID.");
            }
            Models.User? currentUser = _context.Users.FirstOrDefault(u => u.Id == parsedUserId);
            if (currentUser == null)
            {
                throw new ArgumentException("Authenticated user not found.");
            }
            Report report = new()
            {
                Title = reportRequest.Title,
                Description = reportRequest.Description,
                CategoryId = reportRequest.CategoryId,
                Location = reportRequest.Location,
                LocationDetail = reportRequest.LocationDetail,
                UserId = currentUser.Id,
                CreatedAt = DateTime.UtcNow
            };
            _context.Reports.Add(report);
            _context.SaveChanges();
            return new ReportResponse
            {
                StatusCode = (int)HttpStatusCode.Created,
                Message = "Report created successfully.",
                Report = report
            };
        }
        public ReportResponse DeleteReport(int idRep, int idUser, string jwt, ApplicationDbContext _context)
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
                Report = report
            };
        }
        public ReportResponse UpdateReport(int idRep, ReportRequest updateRequest, string jwt, ApplicationDbContext _context)
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
            // Aggiorna i campi del report
            report.Title = updateRequest.Title;
            report.Description = updateRequest.Description;
            report.CategoryId = updateRequest.CategoryId;
            report.Location = updateRequest.Location;
            report.LocationDetail = updateRequest.LocationDetail;
            _context.Reports.Update(report);
            _context.SaveChanges();
            return new ReportResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Report updated successfully.",
                Report = report
            };
        }
    }
}
