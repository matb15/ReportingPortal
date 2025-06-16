using Models.enums;

namespace Models.http
{
    public class Response
    {
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
    }

    public class Cluster<T>
    {
        public List<T> Items { get; set; } = [];
    }
    public class ClusterResponse : Response
    {
        public List<Cluster<ReportDto>> Clusters { get; set; } = [];
    }

    public class ReportSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public ReportStatusEnum Status { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }


    public class ReportCluster
    {
        public int Count { get; set; }
        public double CenterLat { get; set; }
        public double CenterLng { get; set; }
        public List<ReportSummaryDto>? Reports { get; set; } // null if > 5 reports
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

    public class CategoryResponse : Response
    {
        public Category Category { get; set; } = default!;
    }

    public class ReportDto : Request
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public int UserId { get; set; }

        public string Location { get; set; } = string.Empty;

        public string LocationDetail { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ReportStatusEnum Status { get; set; } = ReportStatusEnum.Pending;
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public int? FileId { get; set; } = null;

        public UploadFile? File { get; set; } = null;

        public User? User { get; set; } = default!;

        public Category? Category { get; set; } = default!;
    }

    public class ReportResponse : Response
    {
        public ReportDto Report { get; set; } = default!;
    }
    public class NotificationResponse : Response
    {
        public Notification Notification { get; set; } = default!;
    }
    public class VerificationTokenResponse : Response
    {
        public bool IsValid { get; set; } = false;
    }

    public class NotificationConnectResponse : Response
    {

    }

    public class VerificationTokenRetryReponse : Response
    {
    }

    public class ReportsPaginatedResponse : PagedResponse<ReportDto>
    {

    }

    public class UsersPaginatedResponse : PagedResponse<User>
    {
    }

    public class CategoriesPaginatedResponse : PagedResponse<Category>
    {
    }

    public class NotificationsPaginatedResponse : PagedResponse<Notification>
    {
    }

    public class ReportReplyResponse : Response 
    {
        public ReportReply reportReply { get; set; } = default!;
    }

}
