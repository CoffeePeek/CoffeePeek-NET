namespace CoffeePeek.Account.Application.Features.Auth.Password.ResetPassword;

public record ResetPasswordCommand(string Token, string NewPassword);
