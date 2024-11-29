using System.ComponentModel.DataAnnotations;

namespace TokenService.Models.DataModels;

public class RefreshToken
{
    [Key]
    public long Id { get; set; }
    public string? UserName { get; set; }
    public string? Token { get; set; }
    public DateTime TokenExpiryTime { get; set; }
    public bool IsRevoked { get; set; }
}