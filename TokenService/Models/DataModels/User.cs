using System.ComponentModel.DataAnnotations;

namespace TokenService.Models.DataModels;

public class User
{
    [Key]
    public long Id { get; set; }
    public string? UserName { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
    public bool IsRevoked { get; set; }
}