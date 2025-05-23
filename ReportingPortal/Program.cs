using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ReportingPortal.Services;

namespace ReportingPortal
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            string? apiBase = builder.Configuration["ApiBaseAddress"];
            if (string.IsNullOrWhiteSpace(apiBase))
            {
                throw new InvalidOperationException("ApiBaseAddress configuration is missing or empty.");
            }

            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri(apiBase)
            });

            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<ReportsService>();
            builder.Services.AddScoped<NotificationsService>();

            builder.Services.AddBlazoredLocalStorage();

            await builder.Build().RunAsync();
        }
    }
}
