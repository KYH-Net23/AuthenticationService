using System.ComponentModel.DataAnnotations;

namespace TokenService.Models.FormModels;

public record LoginModel
{
    [Required]
    public string Email { get; init; } = null!;
    [Required]
    public string Password { get; init; } = null!;
}