namespace CoffeePeek.AuthService.Models;

public class SignInResultWrapper
{
    public SignInResult Result { get; }
    public static SignInResultWrapper Success => new(SignInResult.Success);
    public static SignInResultWrapper Failed => new(SignInResult.Failed);
    public static SignInResultWrapper RequiresTwoFactor => new(SignInResult.RequiresTwoFactor);
    public static SignInResultWrapper NotAllowed => new(SignInResult.NotAllowed);

    private SignInResultWrapper(SignInResult result) => Result = result;
}