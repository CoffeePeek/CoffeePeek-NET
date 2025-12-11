using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.JobVacancies.Configuration;

public class HhApiOptions
{
    public string BaseUrl { get; set; } = "https://api.hh.ru/";
    public string UserAgent { get; set; } = "CoffeePeek/1.0 (stefisen@yandex.ru(test)))";
    public int TimeoutSeconds { get; set; } = 30;
    [Required]
    public string ClientId { get; set; } = "";
    [Required]
    public string ClientSecret { get; set; } = "";
    public string Code { get; set; } = "";
    public string RedirectUri { get; set; } = "";
}

