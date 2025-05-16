using System.ComponentModel.DataAnnotations;

namespace Models.http
{
    public class Request : HttpRequestMessage
    {

    }

    public class LoginRequest : Request
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
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
}
