namespace CoffeePeek.Shared.Infrastructure.Abstract.S3;

public class MinIOOptions
{
    public string BucketName { get; set; } = null!;
    public string Endpoint { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public string AccessKey { get; set; } = null!;
}