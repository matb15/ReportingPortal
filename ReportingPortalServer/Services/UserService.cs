using Models;
using Models.enums;
using Models.front;
using Models.http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ReportingPortalServer.Services
{
    public interface IUserService
    {
        public UserResponse GetMeAsync(string JWT, ApplicationDbContext context);
        public UserResponse UpdateMeAsync(string JWT, UserPutModel updatedUser, ApplicationDbContext context);
        public Response UpdateMePasswordAsync(string JWT, string oldPassword, string newPassword, ApplicationDbContext context);
        public Response DeleteMeAsync(string JWT, ApplicationDbContext context);

        public UserResponse GetUserAsync(string JWT, int id, ApplicationDbContext context);
        public UserResponse UpdateUserAsync(string JWT, int id, UserPutModel updatedUser, ApplicationDbContext context);
        public Response DeleteUserAsync(string JWT, int id, ApplicationDbContext context);
        public PagedResponse<User> GetUserPaginationAsync(string JWT, int page, int pageSize, ApplicationDbContext context);
    }

    public class UserService : IUserService
    {
        public UserResponse GetMeAsync(string JWT, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();

            if (!handler.CanReadToken(JWT))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }

            JwtSecurityToken token = handler.ReadJwtToken(JWT);

            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT user ID is not a valid integer."
                };
            }

            User? user = context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "User not found."
                };
            }

            user.Password = "baldman";
            return new UserResponse
            {
                User = user,
                StatusCode = (int)System.Net.HttpStatusCode.OK,
            };
        }
        public UserResponse UpdateMeAsync(string JWT, UserPutModel updatedUser, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(JWT))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }

            JwtSecurityToken token = handler.ReadJwtToken(JWT);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }

            User? user = context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "User not found."
                };
            }

            user.Name = updatedUser.Name;
            user.Surname = updatedUser.Surname;
            user.Role = updatedUser.Role;
            user.UpdatedAt = DateTime.UtcNow;

            context.SaveChanges();

            user.Password = "baldman";
            return new UserResponse
            {
                User = user,
                StatusCode = (int)System.Net.HttpStatusCode.OK,
            };
        }
        public Response UpdateMePasswordAsync(string JWT, string oldPassword, string newPassword, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(JWT))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }

            JwtSecurityToken token = handler.ReadJwtToken(JWT);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }

            User? user = context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "User not found."
                };
            }

            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.Password))
            {
                return new Response
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Old password is incorrect."
                };
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            context.SaveChanges();

            user.Password = "baldman";
            return new Response
            {
                StatusCode = (int)System.Net.HttpStatusCode.OK,
            };
        }
        public Response DeleteMeAsync(string JWT, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(JWT))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }

            JwtSecurityToken token = handler.ReadJwtToken(JWT);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }

            User? user = context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "User not found."
                };
            }

            context.Users.Remove(user);
            context.SaveChanges();

            return new Response
            {
                StatusCode = (int)System.Net.HttpStatusCode.OK,
                Message = "Account eliminato con successo."
            };
        }

        public UserResponse GetUserAsync(string JWT, int id, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(JWT))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }
            JwtSecurityToken token = handler.ReadJwtToken(JWT);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }

            User? currentUser = context.Users.FirstOrDefault(u => u.Id == userId);
            if (currentUser == null)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }

            if (currentUser.Role != UserRoleEnum.Admin)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Forbidden,
                    Message = "Solo gli amministratori possono accedere a questa risorsa."
                };
            }

            User? user = context.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "User not found."
                };
            }

            user.Password = "baldman";
            return new UserResponse
            {
                User = user,
                StatusCode = (int)System.Net.HttpStatusCode.OK,
            };
        }
        public UserResponse UpdateUserAsync(string JWT, int id, UserPutModel updatedUser, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(JWT))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }

            JwtSecurityToken token = handler.ReadJwtToken(JWT);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }

            User? currentUser = context.Users.FirstOrDefault(u => u.Id == userId);
            if (currentUser == null)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }

            if (currentUser.Role != UserRoleEnum.Admin)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Forbidden,
                    Message = "Solo gli amministratori possono accedere a questa risorsa."
                };
            }

            User? user = context.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "User not found."
                };
            }
            user.Name = updatedUser.Name;
            user.Surname = updatedUser.Surname;
            user.Role = updatedUser.Role;
            user.UpdatedAt = DateTime.UtcNow;
            context.SaveChanges();

            user.Password = "baldman";

            return new UserResponse
            {
                User = user,
                StatusCode = (int)System.Net.HttpStatusCode.OK,
            };
        }
        public Response DeleteUserAsync(string JWT, int id, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(JWT))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }

            JwtSecurityToken token = handler.ReadJwtToken(JWT);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }

            User? currentUser = context.Users.FirstOrDefault(u => u.Id == userId);
            if (currentUser == null)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }

            if (currentUser.Role != UserRoleEnum.Admin)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Forbidden,
                    Message = "Solo gli amministratori possono accedere a questa risorsa."
                };
            }

            User? user = context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "User not found."
                };
            }

            context.Users.Remove(user);
            context.SaveChanges();

            return new Response
            {
                StatusCode = (int)System.Net.HttpStatusCode.OK,
                Message = "User deleted successfully."
            };
        }
        public PagedResponse<User> GetUserPaginationAsync(string JWT, int page, int pageSize, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(JWT))
            {
                return new PagedResponse<User>
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }

            JwtSecurityToken token = handler.ReadJwtToken(JWT);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return new PagedResponse<User>
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }

            User? currentUser = context.Users.FirstOrDefault(u => u.Id == userId);
            if (currentUser == null)
            {
                return new PagedResponse<User>
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }
            
            if (currentUser.Role != UserRoleEnum.Admin)
            {
                return new PagedResponse<User>
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Forbidden,
                    Message = "Solo gli amministratori possono accedere a questa risorsa."
                };
            }

            int totalCount = context.Users.Count();
            List<User> users = [.. context.Users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsEnumerable()
                .Select(u => { u.Password = "baldman"; return u; })];

            return new PagedResponse<User>
            {
                Items = users,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                StatusCode = (int)System.Net.HttpStatusCode.OK
            };
        }
    }
}
