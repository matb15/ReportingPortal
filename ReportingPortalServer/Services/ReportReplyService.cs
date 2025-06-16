using Models;
using Models.enums;
using Models.http;
using ReportingPortalServer.Services.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace ReportingPortalServer.Services
{
    public interface IReportReplyService
    {
        public ReportReplyResponse CreateReportReply(CreateReportReplyRequest request, ApplicationDbContext context, string jwt);
        public ReportReplyResponse DeleteReportReply(int idRep, string jwt, ApplicationDbContext _context);
        public ReportReplyResponse UpdateReportReply(int idRep, string mess, string jwt, ApplicationDbContext _context);
    }

    public class ReportReplyService : IReportReplyService
    {
        public ReportReplyResponse CreateReportReply(CreateReportReplyRequest request, ApplicationDbContext context, string jwt)
        {
            if (request == null)
            {
                return new ReportReplyResponse
                {
                    StatusCode = 400,
                    Message = "Request cannot be null."
                };
            }
            if (string.IsNullOrEmpty(jwt))
            {
                return new ReportReplyResponse
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }
            if (request.ReportId <= 0)
            {
                return new ReportReplyResponse
                {
                    StatusCode = 400,
                    Message = "Invalid report reply data."
                };
            }
            var reply = new ReportReply
            {
                Id = request.Id,
                ReportId = request.ReportId,
                UserId = request.UserId,
                Message = request.Message,
                NewStatus = Models.enums.ReportStatusEnum.Pending
            };
            context.ReportReplies.Add(reply);
            context.SaveChanges();
            return new ReportReplyResponse
            {
                StatusCode = 201,
                Message = "Report reply created successfully.",
                reportReply = reply 
            };
        }

        public ReportReplyResponse DeleteReportReply(int idRep, string jwt, ApplicationDbContext _context)
        {
            if (string.IsNullOrEmpty(jwt))
            {
                return new ReportReplyResponse
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }
            if (idRep <= 0)
            {
                return new ReportReplyResponse
                {
                    StatusCode = 400,
                    Message = "Invalid report reply ID."
                };
            }

            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(jwt))
            {
                return new ReportReplyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }
            var token = handler.ReadJwtToken(jwt);
            var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int parsedUserId))
            {
                return new ReportReplyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }
            var currentUser = _context.Users.FirstOrDefault(u => u.Id == parsedUserId);
            if (currentUser == null)
            {
                return new ReportReplyResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }

            ReportReply? reportReply = currentUser.Role == UserRoleEnum.Admin
                ? _context.ReportReplies.FirstOrDefault(r => r.Id == idRep)
                : _context.ReportReplies.FirstOrDefault(r => r.Id == idRep && r.UserId == currentUser.Id);

            if (reportReply == null)
            {
                return new ReportReplyResponse
                {
                    StatusCode = 404,
                    Message = "Report reply not found or you do not have permission."
                };
            }

            _context.ReportReplies.Remove(reportReply);
            _context.SaveChanges();
            return new ReportReplyResponse
            {
                reportReply = reportReply,
                StatusCode = 200,
                Message = "Report reply deleted successfully."
            };
        }

        public ReportReplyResponse UpdateReportReply(int idRep, string mess, string jwt, ApplicationDbContext _context)
        {
            if (string.IsNullOrEmpty(jwt))
            {
                return new ReportReplyResponse
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }
            if (idRep <= 0)
            {
                return new ReportReplyResponse
                {
                    StatusCode = 400,
                    Message = "Invalid report reply ID."
                };
            }

            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(jwt))
            {
                return new ReportReplyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }
            var token = handler.ReadJwtToken(jwt);
            var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int parsedUserId))
            {
                return new ReportReplyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }
            var currentUser = _context.Users.FirstOrDefault(u => u.Id == parsedUserId);
            if (currentUser == null)
            {
                return new ReportReplyResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }

            ReportReply? reportReply = currentUser.Role == UserRoleEnum.Admin
                ? _context.ReportReplies.FirstOrDefault(r => r.Id == idRep)
                : _context.ReportReplies.FirstOrDefault(r => r.Id == idRep && r.UserId == currentUser.Id);

            if (reportReply == null)
            {
                return new ReportReplyResponse
                {
                    StatusCode = 404,
                    Message = "Report reply not found or you do not have permission."
                };
            }
            reportReply.Message = mess;
            _context.SaveChanges();
            return new ReportReplyResponse
            {
                reportReply = reportReply,
                StatusCode = 200,
                Message = "Report reply updated successfully."
            };
        }
    }
}
