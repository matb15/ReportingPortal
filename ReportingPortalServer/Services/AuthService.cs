using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Threading.Tasks;
using Models;

namespace ReportingPortalServer.Services
{
    public interface IAuthService
    {
        public List<User> LoginAsync(string username, string password, ApplicationDbContext context);
    }

    public class AuthService : IAuthService
    {
        public List<User> LoginAsync(string username, string password, ApplicationDbContext context)
        {
            return context.Users.ToList();
        }
    }
}
