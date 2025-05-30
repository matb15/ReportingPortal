using Models;
using Models.enums;
using Models.front;
using Models.http;
using System.Net;
using WebPush;

namespace ReportingPortalServer.Services
{
    public interface INotificationService
    {
        NotificationResponse SendNotification(int userId, string title, string message, NotificationChannelEnum channel, ApplicationDbContext context);
        NotificationsPaginatedResponse GetUserNotifications(int userId, ApplicationDbContext context, int page = 1, int pageSize = 10);
        NotificationResponse GetNotification(int userId, int notificationId, ApplicationDbContext context);
        NotificationResponse UpdateNotification(int notificationId, NotificationPutModel updatedNotification, ApplicationDbContext context);
        bool MarkAsRead(int notificationId, ApplicationDbContext context);
        bool DeleteNotification(int notificationId, ApplicationDbContext context);
        void CreateNotification(int userId, string title, string message, NotificationChannelEnum channel, ApplicationDbContext context);
        bool SendNotificationPushUser(int userId, string message, ApplicationDbContext context, IConfiguration configuration);
    }

    public class NotificationService(IEmailService emailService) : INotificationService
    {
        private readonly IEmailService _emailService = emailService;

        public NotificationResponse SendNotification(int userId, string title, string message, NotificationChannelEnum channel, ApplicationDbContext context)
        {
            User? user = context.Users.Find(userId);
            if (user == null)
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Utente non trovato."
                };
            }

            Notification notification = new()
            {
                UserId = userId,
                Title = title,
                Message = message,
                Status = NotificationStatusEnum.Unread,
                Channel = channel,
                CreatedAt = DateTime.UtcNow
            };

            context.Notifications.Add(notification);
            context.SaveChanges();

            return new NotificationResponse
            {
                StatusCode = (int)HttpStatusCode.Created,
                Message = "Notification created successfully",
                Notification = notification
            };
        }

        public NotificationsPaginatedResponse GetUserNotifications(int userId, ApplicationDbContext context, int page = 1, int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            IQueryable<Notification> query = context.Notifications
                .Where(n => n.UserId == userId && n.Channel == NotificationChannelEnum.App)
                .OrderByDescending(n => n.CreatedAt);

            int totalNotifications = query.Count();

            List<Notification> notifications = [.. query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)];

            return new NotificationsPaginatedResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Notifications retrieved successfully",
                Page = page,
                PageSize = pageSize,
                TotalCount = totalNotifications,
                Items = notifications
            };
        }

        public NotificationResponse GetNotification(int userId, int notificationId, ApplicationDbContext context)
        {
            Notification? notification = context.Notifications
                .Where(n => n.UserId == userId && n.Id == notificationId)
                .FirstOrDefault();

            if (notification == null)
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Notification not found."
                };
            }

            return new NotificationResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Notification retrieved successfully",
                Notification = notification
            };
        }

        public NotificationResponse UpdateNotification(int notificationId, NotificationPutModel updatedNotification, ApplicationDbContext context)
        {
            Notification? notification = context.Notifications.Find(notificationId);
            if (notification == null)
            {
                return new NotificationResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Notification not found."
                };
            }

            notification.Status = updatedNotification.Status;
            notification.ReadAt = updatedNotification.ReadAt;
            notification.DismissedAt = updatedNotification.DismissedAt;
            notification.EmailSent = updatedNotification.EmailSent;
            notification.EmailSentAt = updatedNotification.EmailSentAt;
            notification.PushSent = updatedNotification.PushSent;
            notification.PushSentAt = updatedNotification.PushSentAt;

            context.SaveChanges();

            return new NotificationResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Notification updated successfully",
                Notification = notification
            };
        }

        public bool MarkAsRead(int notificationId, ApplicationDbContext context)
        {
            Notification? notification = context.Notifications.Find(notificationId);
            if (notification == null) return false;

            notification.Status = NotificationStatusEnum.Read;
            notification.ReadAt = DateTime.UtcNow;
            context.SaveChanges();
            return true;
        }

        public bool DeleteNotification(int notificationId, ApplicationDbContext context)
        {
            Notification? notification = context.Notifications.Find(notificationId);
            if (notification == null) return false;

            context.Notifications.Remove(notification);
            context.SaveChanges();
            return true;
        }

        public void CreateNotification(int userId, string title, string message, NotificationChannelEnum channel, ApplicationDbContext context)
        {
            Notification notification = new()
            {
                UserId = userId,
                Title = title,
                Message = message,
                Channel = channel,
                Status = NotificationStatusEnum.Unread,
                CreatedAt = DateTime.UtcNow
            };

            context.Notifications.Add(notification);
            context.SaveChanges();
        }
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
    }
}

