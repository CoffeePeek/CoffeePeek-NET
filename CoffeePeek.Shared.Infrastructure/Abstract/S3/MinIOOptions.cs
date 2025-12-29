using System.ComponentModel.DataAnnotations;

namespace CoffeePeek.Moderation.Infrastructure;

public class MinIOOptions
{
    [Required] public string BucketName { get; set; }
    [Required] public string Endpoint { get; set; }
    [Required] public string SecretKey { get; set; }
    [Required] public string AccessKey { get; set; }
}