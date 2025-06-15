using Microsoft.AspNetCore.Mvc;
using Models.http;
using ReportingPortalServer.Services;
using ReportingPortalServer.Services.Helpers;

namespace ReportingPortalServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController(ILogger<ReportController> logger, ApplicationDbContext context, IReportService reportService) : Controller
    {
        private readonly ILogger<ReportController> _logger = logger;
        private readonly ApplicationDbContext _context = context;
        private readonly IReportService _reportService = reportService;

        [HttpGet("{id}")]
        public ReportResponse GetReportById(int id)
        {
            _logger.LogInformation($"GetReportById request received for id: {id}");

            if (id <= 0)
            {
                return new ReportResponse
                {
                    StatusCode = 400,
                    Message = "Invalid report ID."
                };
            }

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new ReportResponse
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return _reportService.GetReportById(jwt, id, _context);
        }

        [HttpGet]
        public async Task<ReportsPaginatedResponse> GetPaginated([FromQuery] ReportsPaginatedRequest req)
        {
            _logger.LogInformation($"GetPaginated request received with Page={req.Page}, PageSize={req.PageSize}");

            if (req.PageSize <= 0 || req.Page < 0)
            {
                return new ReportsPaginatedResponse
                {
                    StatusCode = 400,
                    Message = "Invalid pagination parameters."
                };
            }

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new ReportsPaginatedResponse
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return await _reportService.GetPaginatedReports(jwt, req, _context);
        }

        [HttpPost]
        public ReportResponse CreateReport([FromBody] CreateReportRequest reportRequest)
        {
            _logger.LogInformation("CreateReport request received");

            if (reportRequest == null || string.IsNullOrEmpty(reportRequest.Title) || string.IsNullOrEmpty(reportRequest.Description))
            {
                return new ReportResponse
                {
                    StatusCode = 400,
                    Message = "Invalid request data."
                };
            }

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new ReportResponse
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return _reportService.CreateReport(reportRequest, jwt, _context);
        }

        [HttpDelete("{id}")]
        public ReportResponse DeleteReport(int id)
        {
            _logger.LogInformation($"DeleteReport request received for idRep: {id}");

            if (id <= 0)
            {
                return new ReportResponse
                {
                    StatusCode = 400,
                    Message = "Invalid report ID."
                };
            }

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new ReportResponse
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return _reportService.DeleteReport(id, jwt, _context);
        }

        [HttpPut("{id}")]
        public ReportResponse UpdateReport(int id, [FromBody] CreateReportRequest updateRequest)
        {
            _logger.LogInformation($"UpdateReport request received for idRep: {id}");

            if (id <= 0)
            {
                return new ReportResponse
                {
                    StatusCode = 400,
                    Message = "Invalid report ID."
                };
            }

            if (updateRequest == null)
            {
                return new ReportResponse
                {
                    StatusCode = 400,
                    Message = "Invalid update data."
                };
            }

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new ReportResponse
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return _reportService.UpdateReport(id, updateRequest, jwt, _context);
        }
    }
}
