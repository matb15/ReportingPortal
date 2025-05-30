using Microsoft.EntityFrameworkCore;
using Models;

namespace ReportingPortalServer.Services
{
    public class ApplicationDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
        public DbSet<PushSubscription> PushSubscriptions { get; set; }
    }
}
