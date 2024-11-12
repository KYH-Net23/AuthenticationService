using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TokenService.Models;

public class RefreshTokenModel
{
    [Key]
    public long Id { get; set; }
    public string? UserName { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
}