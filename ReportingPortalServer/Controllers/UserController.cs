using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.front;
using Models.http;
using ReportingPortalServer.Services;
using ReportingPortalServer.Services.Helpers;

namespace ReportingPortalServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(ILogger<UserController> logger, IUserService userService, ApplicationDbContext context) : Controller
    {
        private readonly ILogger<UserController> _logger = logger;
        private readonly IUserService _userService = userService;
        private readonly ApplicationDbContext context = context;

        [HttpGet("me")]
        public UserResponse GetMe()
        {
            _logger.LogInformation("GetMe request received");

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return _userService.GetMeAsync(jwt, context);
        }

        [HttpPut("me")]
        public UserResponse UpdateMe(UserPutModel user)
        {
            _logger.LogInformation("UpdateMe request received");

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return _userService.UpdateMeAsync(jwt, user, context);
        }

        [HttpPut("me/password")]
        public Response UpdateMePassword(ChangePasswordFormModel changePasswordForm)
        {
            _logger.LogInformation("UpdateMePassword request received");

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return _userService.UpdateMePasswordAsync(jwt, changePasswordForm.CurrentPassword, changePasswordForm.NewPassword, context);
        }

        [HttpDelete("me")]
        public Response DeleteMe()
        {
            _logger.LogInformation("DeleteMe request received");

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return _userService.DeleteMeAsync(jwt, context);
        }

        [HttpGet("getUser")]
        [Authorize]
        public UserResponse GetUser(int userId)
        {
            _logger.LogInformation($"GetUser request received for user ID: {userId}");
            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }
            return _userService.GetUserAsync(jwt, userId, context);
        }

        [HttpGet("getUserPagination")]
        [Authorize]
        public PagedResponse<User> GetUserPagination(int page, int pageSize)
        {
            _logger.LogInformation($"GetUserPagination request received for page: {page}, pageSize: {pageSize}");
            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new PagedResponse<User>
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }
            return _userService.GetUserPaginationAsync(jwt, page, pageSize, context);
        }

        [HttpPut("updateUser")]
        [Authorize]
        public Response PutUser(int userId, UserPutModel user)
        {
            _logger.LogInformation($"PutUser request received for user ID: {userId}");
            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }
            return _userService.UpdateUserAsync(jwt, userId, user, context);
        }

        [HttpDelete("deleteUser")]
        [Authorize]
        public Response DeleteUser(int userId) 
        {
            _logger.LogInformation($"DeleteUser request received for user ID: {userId}");
            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new UserResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }
            return _userService.DeleteUserAsync(jwt, userId, context);
        }
    }
}
