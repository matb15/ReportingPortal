namespace Models.http
{
    public class Response : HttpResponseMessage
    {
        public required string Message { get; set; }
    }

    public class PagedResponse<T> : Response
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public List<T> Items { get; set; } = [];
    }

    public class RegisterResponse
    {
        public required string Message { get; set; }
        public int StatusCode { get; set; }
    }

    public class LoginResponse 
    {
        public string Token { get; set; } = default!;
        public User User { get; set; } = default!;
        public required string Message { get; set; }
        public int StatusCode { get; set; }
    }

    public class ReportsPaginatedResponse : PagedResponse<Report>
    {

    }

    public class ReportResponse : Response
    {
        public Report Report { get; set; } = default!;
    }

    public class NotificationsPaginatedResponse : PagedResponse<Notification>
    {
    }

    public class GenericResponse
    {
        public required string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
