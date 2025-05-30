using Microsoft.AspNetCore.Mvc;
using Models;
using Models.front;
using Models.http;
using ReportingPortalServer.Services;
using ReportingPortalServer.Services.Helpers;

namespace ReportingPortalServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController(ILogger<NotificationController> logger, ApplicationDbContext context) : Controller
    {
        private readonly ILogger<NotificationController> _logger = logger;
        private readonly ApplicationDbContext context = context;

        [HttpPost("connect-push-notifications")]
        public NotificationConnectResponse ConnectPushNotifications(NotificationConnectRequest request)
        {
            if (request == null)
            {
                return new NotificationConnectResponse
                {
                    Message = "Invalid request data.",
                    StatusCode = 400,
                };
            }

            User? user = context.Users.FirstOrDefault(u => u.Email == request.Client);
            if (user == null)
            {
                return new NotificationConnectResponse
                {
                    Message = "User not found.",
                    StatusCode = 404,
                };
            }

            PushSubscription subscription = new()
            {
                Client = request.Client,
                Endpoint = request.Endpoint,
                P256dh = request.P256dh,
                Auth = request.Auth,
                UserId = user.Id
            };

            context.PushSubscriptions.Add(subscription);
            context.SaveChanges();

            return new NotificationConnectResponse
            {
                Message = "Push notification subscription created successfully.",
                StatusCode = 200,
            };
        }
    }
}
