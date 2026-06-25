using CoffeePeek.Shared.Auth.Constants;

namespace CoffeePeek.Shared.Auth;

public static class GatewayAuthHttpExtensions
{
    public static void AddGatewayAuthHeader(this HttpRequestMessage request, string secretKey)
    {
        if (string.IsNullOrWhiteSpace(secretKey))
            return;

        request.Headers.Remove(GatewayHeaderConsts.XGatewayAuth);
        request.Headers.TryAddWithoutValidation(GatewayHeaderConsts.XGatewayAuth, secretKey);
    }
}
