using Models;
using Models.enums;
namespace ReportingPortalServer.Services.Jobs
{
    public class NotificationPushSender(IServiceScopeFactory scopeFactory, ILogger<NotificationPushSender> logger) : IScheduledJob
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<NotificationPushSender> _logger = logger;

        public string JobName => "PushNotificationSender";
        public TimeSpan Interval => TimeSpan.FromMinutes(5);

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

            ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            INotificationService notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            List<Notification> pendingPushNotification = [.. context.Notifications
                .Where(e => e.Channel == NotificationChannelEnum.Push)
                .OrderBy(e => e.PushSent == false || e.PushSent == null)
                .Take(50)];

            _logger.LogInformation("Sending {Count} pending push notifications...", pendingPushNotification.Count);

            foreach (Notification email in pendingPushNotification)
            {
                try
                {
                    User? user = context.Users.Find(email.UserId);
                    if (user == null)
                    {
                        email.PushSent = true;

                        context.Notifications.Update(email);
                        continue;
                    }

                    notificationService.SendNotificationPushUser(user.Id, email.Message, context, scope.ServiceProvider.GetRequiredService<IConfiguration>());
                    email.PushSent = true;
                    email.PushSentAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email to {Email}", email.UserId);
                    email.PushSent = true;
                    email.PushSentAt = DateTime.UtcNow;
                }
            }
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
