using System.ComponentModel.DataAnnotations;

namespace Models.http
{
    public class Request : HttpRequestMessage
    {

    }

    public class PagedRequest
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;
    }

    public class LoginRequest : Request
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string Password { get; set; } = default!;
    }

    public class RegisterRequest : Request
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string Password { get; set; } = default!;

        [Required]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = default!;

        [Required]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string Name { get; set; } = default!;
        [Required]
        [StringLength(100, ErrorMessage = "Surname cannot be longer than 100 characters.")]
        public string Surname { get; set; } = default!;
    }

    public class ReportRequest : Request
    {
        [Required]
        public string Title { get; set; } = default!;
        [Required]
        public string Description { get; set; } = default!;
        [Required]
        public int CategoryId { get; set; } = default!;
        [Required]
        public string Location { get; set; } = default!;
        public string LocationDetail { get; set; } = default!;
    }

    public class ReportsPaginatedRequest : PagedRequest
    {

    }

    public class UsersPaginatedRequest : PagedRequest
    {
        
    }

    public class NotificationConnectRequest : Request
    {
        [Required]
        public string Client { get; set; } = default!;

        [Required]
        public string Endpoint { get; set; } = default!;

        [Required]
        public string P256dh { get; set; } = default!;

        [Required]
        public string Auth { get; set; } = default!;
    }

    public class NotificationsPaginatedRequest : PagedRequest
    {
        public int UserId { get; set; }
    }

    public class ReadNotificationRequest : Request
    {
        public int UserId { get; set; } = default!;
        public int NotificationId { get; set; } = default!;
    }

    public class VerificationTokenRequest : Request
    {
        public string Token { get; set; } = default!;
    }

    public class VerificationTokenRetryRequest : Request
    {
        public int UserId { get; set; } = default!;
    }
}
