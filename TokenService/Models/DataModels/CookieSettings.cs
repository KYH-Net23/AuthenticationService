namespace TokenService.Models.DataModels;

public class CookieSettings
{
    public int RefreshTokenCookieDurationInHours { get; init; }
    public int AccessTokenCookieDurationInHours { get; init; }
}