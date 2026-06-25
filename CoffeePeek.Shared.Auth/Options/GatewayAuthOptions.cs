using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.Shared.Auth.Options;

public class GatewayAuthOptions
{
    public const string SectionName = "GatewayAuth";

    [Required]
    [MinLength(32)]
    public string SecretKey { get; set; } = null!;
}
