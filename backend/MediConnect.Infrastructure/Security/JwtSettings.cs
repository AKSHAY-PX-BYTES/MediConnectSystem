namespace MediConnect.Infrastructure.Security;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "MediConnect";
    public string Audience { get; set; } = "MediConnectClients";
    public string Secret { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 7;
}
