using Microsoft.EntityFrameworkCore;
using ReportingPortalServer.Services;

namespace ReportingPortalServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            string? origins = "origins";
            var frontAddress = builder.Configuration["FrontAddress"];
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

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<AuthService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
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
