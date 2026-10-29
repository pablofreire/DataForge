using System.ComponentModel.DataAnnotations;

namespace DataForge.Modules.Identity.Presentation.Models;

public sealed class LoginFormModel
{
    [Required(ErrorMessage = "Enter your e-mail address.")]
    [EmailAddress(ErrorMessage = "Enter a valid e-mail address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter your password.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
