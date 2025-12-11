namespace CoffeePeek.JobVacancies.Constants;

public static class CitiesConsts
{
    public static Guid DefaultCityId = Guid.Parse("D3FE962F-B1AA-42C3-B3B0-EE59322D0B6B");
    
    public static readonly Dictionary<Guid, string> Cities = new()
    {
        { Guid.Parse("D3FE962F-B1AA-42C3-B3B0-EE59322D0B6B"), "Минск" },
        { Guid.Parse("39f0b293-ac83-491a-9ef1-8ba060c935d9"), "Москва" },
    };
}
