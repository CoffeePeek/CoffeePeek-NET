namespace CoffeePeek.Contract.Constants;

public static class CitiesConsts
{
    public static readonly Guid MinskId = Guid.Parse("D3FE962F-B1AA-42C3-B3B0-EE59322D0B6B");
    public static readonly Guid MoscowId = Guid.Parse("39f0b293-ac83-491a-9ef1-8ba060c935d9");

    public static readonly Dictionary<Guid, string> Cities = new()
    {
        { MinskId, "Минск" },
        { MoscowId, "Москва" },
    };
}