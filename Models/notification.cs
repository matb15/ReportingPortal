using Models.enums;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; } = default!;

        [Required, MaxLength(500)]
        public string Message { get; set; } = default!;

        [Required]
        public NotificationStatusEnum Status { get; set; } = NotificationStatusEnum.Unread;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; } = null;

        public DateTime? DeletedAt { get; set; } = null;
    }
}
