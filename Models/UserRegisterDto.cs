using System.ComponentModel.DataAnnotations;

namespace VerifyEmailForgotPasswordTut.Models
{
    public class UserRegisterDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6, ErrorMessage ="Please enter atleast 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Required, Compare("Password")]
        public string ConfirmPassword {  get; set; } = string.Empty;
    }
}
