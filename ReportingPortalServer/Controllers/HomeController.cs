using Microsoft.AspNetCore.Mvc;
using ReportingPortalServer.Services;

namespace ReportingPortalServer.Controllers
{
    [ApiController]
    [Route("/")]
    public class HomeController(INotificationService notificationService, ApplicationDbContext applicationDbContext, IConfiguration configuration) : ControllerBase
    {
        private readonly INotificationService _notificationService = notificationService;
        private readonly ApplicationDbContext _applicationDbContext = applicationDbContext;
        private readonly IConfiguration _configuration = configuration;

        [HttpGet]
        public IActionResult GetStatus()
        {
            _notificationService.SendNotificationPushUser(2, "Test notification", _applicationDbContext, _configuration);

            return Ok(new { status = "API is running." });

        }
    }
}
