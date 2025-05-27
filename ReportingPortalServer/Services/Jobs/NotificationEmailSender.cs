using Models;
using Models.enums;
namespace ReportingPortalServer.Services.Jobs
{
    public class NotificationEmailSender(IServiceScopeFactory scopeFactory, ILogger<NotificationEmailSender> logger) : IScheduledJob
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<NotificationEmailSender> _logger = logger;

        public string JobName => "EmailNotificationSender";
        public TimeSpan Interval => TimeSpan.FromMinutes(5);

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

            ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            List<Notification> pendingEmails = [.. context.Notifications
                .Where(e => e.Channel == NotificationChannelEnum.Email)
                .OrderBy(e => e.EmailSent == false || e.EmailSent == null)
                .Take(50)];

            _logger.LogInformation("Sending {Count} pending emails...", pendingEmails.Count);

            foreach (Notification email in pendingEmails)
            {
                try
                {
                    User? user = context.Users.Find(email.UserId);
                    if (user == null) {
                        email.EmailSent = true;

                        context.Notifications.Update(email);
                        continue;
                    }

                    emailService.SendEmail(user.Email, email.Title, email.Message);

                    email.EmailSent = true;
                    email.EmailSentAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email to {Email}", email.UserId);
                    email.EmailSent = true;
                    email.EmailSentAt = DateTime.UtcNow;
                }
            }
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
