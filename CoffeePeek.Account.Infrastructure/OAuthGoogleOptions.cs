using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.Account.Infrastructure;

public class OAuthGoogleOptions
{
    [Required(AllowEmptyStrings = false)]
    public string ClientId { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string ClientSecret { get; set; } = string.Empty;
}
