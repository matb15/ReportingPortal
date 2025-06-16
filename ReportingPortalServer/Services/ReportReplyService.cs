using Models;
using Models.http;
using ReportingPortalServer.Services.Helpers;

namespace ReportingPortalServer.Services
{
    public interface IReportReplyService
    {
        public ReportReplyResponse CreateReportReply(CreateReportReplyRequest request, ApplicationDbContext context, string jwt);
        public ReportReplyResponse DeleteReportReply(int idRep, int idUtente, string jwt, ApplicationDbContext _context);
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

        public ReportReplyResponse DeleteReportReply(int idRep, int idUtente, string jwt, ApplicationDbContext _context)
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
            var reply = _context.ReportReplies.Find(idRep);
            if (reply == null)
            {
                return new ReportReplyResponse
                {
                    StatusCode = 404,
                    Message = "Report reply not found."
                };
            }
            if (reply.UserId != idUtente)
            {
                return new ReportReplyResponse
                {
                    StatusCode = 403,
                    Message = "You do not have permission to delete this report reply."
                };
            }
            _context.ReportReplies.Remove(reply);
            _context.SaveChanges();
            return new ReportReplyResponse
            {
                reportReply = reply,
                StatusCode = 200,
                Message = "Report reply deleted successfully."
            };
        }
    }
}
