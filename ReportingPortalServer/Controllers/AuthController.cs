using Microsoft.AspNetCore.Mvc;
using Models.http;
using ReportingPortalServer.Services;

namespace ReportingPortalServer.Controllers
{               
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly AuthService _authService;
        private readonly ApplicationDbContext context;

        public AuthController(ILogger<AuthController> logger, AuthService authService, ApplicationDbContext context)
        {
            _logger = logger;
            _authService = authService;
            this.context = context;
        }

        [HttpPost("login")]
        public LoginResponse Login(LoginRequest request)
        {
            _logger.LogInformation("Login request received for user: {Username}", request.Username);
            LoginResponse resposnse = _authService.LoginAsync(request.Username, request.Password, context); 
            return resposnse;
        }

        [HttpPost("register")]
        public RegisterResponse Register(RegisterRequest request)
        {
            _logger.LogInformation("Register request received for user: {Email}", request.Email);
            var response = _authService.RegisterAsync(request, context);
            if (response.IsSuccessStatusCode)
            {
                return response;
            }
            else
            {
                return response;
            }
        }
    }
}
