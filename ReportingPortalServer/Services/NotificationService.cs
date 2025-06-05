using Models;
using Models.enums;
using Models.http;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using WebPush;

namespace ReportingPortalServer.Services
{
    public interface INotificationService
    {
        bool SendNotificationPushUser(int userId, string message, ApplicationDbContext context, IConfiguration configuration);

        NotificationsPaginatedResponse GetNotifications(string jwt, NotificationsPaginatedRequest request, ApplicationDbContext context);
        NotificationResponse ReadNotification(string jwt, int notificationId, ApplicationDbContext context);
        NotificationResponse CreateNotification(string jwt, int UserId, string Message, ApplicationDbContext context);
        NotificationResponse DeleteNotification(string jwt, int notificationId, ApplicationDbContext context);
        NotificationResponse UpdateNotification(string jwt, int notificationId, string title, string message, ApplicationDbContext context);
    }

    public class NotificationService() : INotificationService
    {
        public bool SendNotificationPushUser(int userId, string message, ApplicationDbContext context, IConfiguration configuration)
        {
            User? user = context.Users.Find(userId);
            if (user == null) return false;

            List<WebPush.PushSubscription> subscriptions = [.. context.PushSubscriptions
                .Where(s => s.UserId == userId)
                .Select(s => new WebPush.PushSubscription(s.Endpoint, s.P256dh, s.Auth))];

            string? subject = configuration["VAPID:subject"];
            string? publicKey = configuration["VAPID:publicKey"];
            string? privateKey = configuration["VAPID:privateKey"];

            if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(publicKey) || string.IsNullOrEmpty(privateKey))
            {
                Console.WriteLine("VAPID configuration is missing.");
                return false;
            }

            foreach (WebPush.PushSubscription subscription in subscriptions)
            {
                VapidDetails vapidDetails = new(subject, publicKey, privateKey);
                WebPushClient webPushClient = new();
                try
                {
                    webPushClient.SendNotification(subscription, message, vapidDetails);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending push notification: {ex.Message}");
                    if (ex.InnerException != null)
                        Console.WriteLine("Inner: " + ex.InnerException.Message);
                    return false;
                }
            }

            return true;
        }

        public NotificationsPaginatedResponse GetNotifications(string JWT, NotificationsPaginatedRequest request, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(JWT))
            {
                return new NotificationsPaginatedResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }

            JwtSecurityToken token = handler.ReadJwtToken(JWT);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int parsedUserId))
            {
                return new NotificationsPaginatedResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }

            User? currentUser = context.Users.FirstOrDefault(u => u.Id == parsedUserId);
            if (currentUser == null)
            {
                return new NotificationsPaginatedResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }

            List<Notification> notifications = [];

            if (currentUser.Role != UserRoleEnum.Admin)
            {
                notifications = [.. context.Notifications
                .Where(n => n.UserId == parsedUserId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)];
            }
            else
            {
                notifications = [.. context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)];
            }

            int totalCount = context.Notifications.Count(n => n.UserId == parsedUserId);
            return new NotificationsPaginatedResponse
            {
                Items = notifications,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                StatusCode = (int)HttpStatusCode.OK
            };
        }
        public NotificationResponse ReadNotification(string jwt, int notificationId, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(jwt))
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }
            JwtSecurityToken token = handler.ReadJwtToken(jwt);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int parsedUserId))
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }
            Models.User? currentUser = context.Users.FirstOrDefault(u => u.Id == parsedUserId);
            if (currentUser == null)
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }
            Notification? notification = context.Notifications.FirstOrDefault(n => n.Id == notificationId && n.UserId == parsedUserId);
            if (notification == null)
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Notification not found."
                };
            }

            notification.Status = NotificationStatusEnum.Read;
            notification.ReadAt = DateTime.UtcNow;

            context.SaveChanges();
            return new NotificationResponse
            {
                Notification = notification,
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Notification marked as read."
            };
        }
        public NotificationResponse CreateNotification(string jwt, int UserId, string Message, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(jwt))
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }
            JwtSecurityToken token = handler.ReadJwtToken(jwt);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var parsedUserId))
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }
            Models.User? currentUser = context.Users.FirstOrDefault(u => u.Id == parsedUserId);
            if (currentUser == null)
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }
            if (currentUser.Role != UserRoleEnum.Admin)
            {
                return new NotificationResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Forbidden,
                    Message = "Solo gli amministratori possono accedere a questa risorsa."
                };
            }
            Notification notification = new()
            {
                UserId = UserId,
                Title = "New Notification",
                Message = Message,
                Status = NotificationStatusEnum.Unread,
                Channel = NotificationChannelEnum.App,
                CreatedAt = DateTime.UtcNow
            };
            context.Notifications.Add(notification);
            context.SaveChanges();
            return new NotificationResponse
            {
                Notification = notification,
                StatusCode = (int)HttpStatusCode.Created,
                Message = "Notification created successfully."
            };
        }
        public NotificationResponse DeleteNotification(string jwt, int notificationId, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(jwt))
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }
            JwtSecurityToken token = handler.ReadJwtToken(jwt);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var parsedUserId))
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }
            Models.User? currentUser = context.Users.FirstOrDefault(u => u.Id == parsedUserId);
            if (currentUser == null)
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }
            if (currentUser.Role != UserRoleEnum.Admin)
            {
                return new NotificationResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Forbidden,
                    Message = "Solo gli amministratori possono accedere a questa risorsa."
                };
            }
            Notification? notification = context.Notifications.Find(notificationId);
            if (notification == null)
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Notification not found."
                };
            }
            context.Notifications.Remove(notification);
            context.SaveChanges();
            return new NotificationResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Notification deleted successfully."
            };
        }
        public NotificationResponse UpdateNotification(string jwt, int notificationId, string title, string message, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(jwt))
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }
            JwtSecurityToken token = handler.ReadJwtToken(jwt);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var parsedUserId))
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }
            Models.User? currentUser = context.Users.FirstOrDefault(u => u.Id == parsedUserId);
            if (currentUser == null)
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }
            if (currentUser.Role != UserRoleEnum.Admin)
            {
                return new NotificationResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Forbidden,
                    Message = "Solo gli amministratori possono accedere a questa risorsa."
                };
            }
            Notification? notification = context.Notifications.Find(notificationId);
            if (notification == null)
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Notification not found."
                };
            }
            notification.Title = title;
            notification.Message = message;
            context.SaveChanges();
            return new NotificationResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Notification updated successfully.",
                Notification = notification
            };
        }
    }
}

