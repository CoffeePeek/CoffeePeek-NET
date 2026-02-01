using Asp.Versioning;

namespace CoffeePeek.Shared.Infrastructure.Constants;

public static class ApiVersions
{
    public const string VersionHeader = "X-Api-Version";
    public const string GroupNameFormat = "'v'VVV";
    
    public static ApiVersion Default => V1;
    
    public const string V1_0 = "1";
    public const string V2_0 = "2";

    public static readonly ApiVersion V1 = new(1, 0);
    public static readonly ApiVersion V2 = new(2, 0);
}