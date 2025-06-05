using Microsoft.AspNetCore.Mvc;
using Models;
using Models.http;
using ReportingPortalServer.Services;
using ReportingPortalServer.Services.Helpers;

namespace ReportingPortalServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : Controller
    {
        private readonly ILogger<NotificationController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public NotificationController(ILogger<NotificationController> logger, ApplicationDbContext context, INotificationService notificationService)
        {
            _logger = logger;
            _context = context;
            _notificationService = notificationService;
        }

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

            User? user = _context.Users.FirstOrDefault(u => u.Email == request.Client);
            if (user == null)
            {
                return new NotificationConnectResponse
                {
                    Message = "User not found.",
                    StatusCode = 404,
                };
            }

            PushSubscription? existingSubscription = _context.PushSubscriptions
                .FirstOrDefault(s => s.Client == request.Client && s.Endpoint == request.Endpoint);

            if (existingSubscription == null)
            {
                PushSubscription subscription = new()
                {
                    Client = request.Client,
                    Endpoint = request.Endpoint,
                    P256dh = request.P256dh,
                    Auth = request.Auth,
                    UserId = user.Id
                };

                _context.PushSubscriptions.Add(subscription);
                _context.SaveChanges();

                return new NotificationConnectResponse
                {
                    Message = "Push notification subscription created successfully.",
                    StatusCode = 200,
                };
            }
            else
            {
                return new NotificationConnectResponse
                {
                    Message = "Subscription already exists.",
                    StatusCode = 409,
                };
            }
        }

        [HttpGet("paged")]
        public NotificationsPaginatedResponse GetNotifications([FromQuery] NotificationsPaginatedRequest request)
        {
            if (request == null || request.UserId <= 0)
            {
                return new NotificationsPaginatedResponse
                {
                    Message = "Invalid request data.",
                    StatusCode = 400,
                };
            }

            _logger.LogInformation($"GetNotificationPagination request received for page: {request.Page}, pageSize: {request.PageSize}");
            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new NotificationsPaginatedResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }

            var response = _notificationService.GetNotifications(jwt, request.UserId, request.Page, request.PageSize, _context);
            return new NotificationsPaginatedResponse
            {
                Message = "Notifications retrieved successfully.",
                StatusCode = 200,
                Page = response.Page,
                PageSize = response.PageSize,
                TotalCount = response.TotalCount,
                Items = response.Items
            };
        }

        [HttpPut("read")]
        public NotificationResponse MarkNotificationAsRead([FromBody] ReadNotificationRequest request)
        {
            if (request == null || request.NotificationId <= 0)
            {
                return new NotificationResponse
                {
                    Message = "Invalid request data.",
                    StatusCode = 400,
                };
            }

            _logger.LogInformation($"PutRead request received for notification: {request.NotificationId}, userId: {request.UserId}");
            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new NotificationResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }

            var response = _notificationService.ReadNotification(jwt, request.UserId, request.NotificationId, _context);

            return new NotificationResponse
            {
                Message = "Notification marked as read successfully.",
                StatusCode = 200,
                Notification = response.Notification
            };
        }

        [HttpPost("create")]
        public NotificationResponse CreateNotification([FromBody] CreateNotificationRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Message) || request.UserId <= 0)
            {
                return new NotificationResponse
                {
                    Message = "Invalid request data.",
                    StatusCode = 400,
                };
            }
            _logger.LogInformation($"CreateNotification request received for userId: {request.UserId}, message: {request.Message}");
            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new NotificationResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }
            var response = _notificationService.CreateNotification(jwt, request.UserId, request.Message, _context);
            return new NotificationResponse
            {
                Message = "Notification created successfully.",
                StatusCode = 200,
                Notification = response.Notification
            };
        }
    }
}
