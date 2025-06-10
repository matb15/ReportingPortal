using Microsoft.EntityFrameworkCore;
using Models;
using ReportingPortalServer.Services;
using ReportingPortalServer.Services.AppwriteIO;
using ReportingPortalServer.Services.Jobs;

namespace ReportingPortalServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            string origins = "origins";
            string? frontAddress = builder.Configuration["FrontAddress"];
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(origins,
                    policy =>
                    {
                        if (!string.IsNullOrEmpty(frontAddress))
                        {
                            policy.WithOrigins(frontAddress)
                                  .AllowAnyHeader()
                                  .AllowAnyMethod();
                        }
                    });
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddOpenApi();

            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.Configure<AppwriteSettings>(builder.Configuration.GetSection("AppwriteSettings"));

            builder.Services.AddHostedService<JobSchedulerService>();
            builder.Services.AddSingleton<IAppwriteClient, AppwriteClient>();
            builder.Services.AddSingleton<IUserService, UserService>();
            builder.Services.AddSingleton<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<ITokenVerificationService, TokenVerificationService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddScoped<IScheduledJob, NotificationEmailSender>();
            builder.Services.AddScoped<IScheduledJob, NotificationPushSender>();


            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseCors("origins");

            app.MapControllers();

            app.Run();
        }
    }
}
