using Microsoft.AspNetCore.Mvc;
using Models.http;
using ReportingPortalServer.Services;
using ReportingPortalServer.Services.Helpers;

namespace ReportingPortalServer.Controllers
{
    public class ReportReplyController : Controller
    {
        [ApiController]
        [Route("api/[controller]")]
        public class ReportController(ILogger<ReportReplyController> logger, ApplicationDbContext context, IReportReplyService reportReplyService) : Controller
        {

            private readonly ILogger<ReportReplyController> _logger = logger;
            private readonly ApplicationDbContext _context = context;
            private readonly IReportReplyService _reportReplyService = reportReplyService;

            [HttpPost]
            public ReportReplyResponse CreateReport([FromBody] CreateReportReplyRequest req)
            {
                _logger.LogInformation("CreateReport request received");

                if (req == null || string.IsNullOrEmpty(req.Message))
                {
                    return new ReportReplyResponse
                    {
                        StatusCode = 400,
                        Message = "Invalid request data."
                    };
                }

                string? jwt = Utils.GetJwt(HttpContext);
                if (string.IsNullOrEmpty(jwt))
                {
                    return new ReportReplyResponse
                    {
                        StatusCode = 401,
                        Message = "Authorization header is missing or invalid."
                    };
                }

                return _reportReplyService.CreateReportReply(req, _context, jwt);
            }
        }
    }
}
