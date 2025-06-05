namespace Models.http
{
    public class Response
    {
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
    }

    public class PagedResponse<T> : Response
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public List<T> Items { get; set; } = [];
    }

    public class RegisterResponse : Response
    {
        public int UserId { get; set; }
    }

    public class LoginResponse : Response
    {
        public string Token { get; set; } = default!;
        public User User { get; set; } = default!;
    }

    public class UserResponse : Response
    {
        public User User { get; set; } = default!;
    }

    public class ReportsPaginatedResponse : PagedResponse<Report>
    {

    }

    public class UsersPaginatedResponse : PagedResponse<User>
    {
    }

    public class ReportResponse : Response
    {
        public Report Report { get; set; } = default!;
    }

    public class NotificationsPaginatedResponse : PagedResponse<Notification>
    {
    }

    public class NotificationResponse : Response
    {
        public Notification Notification { get; set; } = default!;
    }

    public class NotificationConnectResponse : Response
    {
       
    }

    public class VerificationTokenResponse : Response
    {
        public bool IsValid { get; set; } = false;
    }

    public class VerificationTokenRetryReponse : Response
    {
    }
}
