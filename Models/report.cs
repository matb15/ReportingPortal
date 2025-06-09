using Models.enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Report
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; } = default!;

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = default!;

        [Required, MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [MaxLength(200)]
        public string LocationDetail { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public ReportStatusEnum Status { get; set; } = ReportStatusEnum.Pending;

        [Required]
        public double Latitude { get; set; } = 0.0;

        [Required]
        public double Longitude { get; set; } = 0.0;

        public List<ReportReply> Replies { get; set; } = [];
    }

}
