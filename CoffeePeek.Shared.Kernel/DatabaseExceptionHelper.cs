namespace CoffeePeek.Shared.Kernel;

public static class DatabaseExceptionHelper
{
    public static bool IsUniqueConstraintViolation(Exception exception)
    {
        for (var ex = exception; ex is not null; ex = ex.InnerException)
        {
            if (ex.Message.Contains("23505", StringComparison.Ordinal)
                || ex.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
