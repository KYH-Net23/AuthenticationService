namespace TokenService.Models;

public record ApiSettings
{
    public string BaseUrl { get; init; } = null!;
}