namespace CoffeePeek.Contract.Requests;

public record UploadUrlRequest( 
    string FileName,
    string ContentType);