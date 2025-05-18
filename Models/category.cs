using System.ComponentModel.DataAnnotations;

namespace Models
{

    public class Category
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string IconClass { get; set; } = "fa-solid fa-circle-exclamation";

        public ICollection<Report> Reports { get; set; } = [];
    }

}
