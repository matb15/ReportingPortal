using Models;
using Models.http;

namespace ReportingPortalServer.Services
{
    public interface IReportReplyService
    {
        public ReportReplyResponse CreateReportReply(CreateReportReplyRequest request, ApplicationDbContext context, string jwt);    
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
    }
}
