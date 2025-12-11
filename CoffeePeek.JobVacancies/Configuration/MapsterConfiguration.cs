using CoffeePeek.JobVacancies.Entities;
using CoffeePeek.JobVacancies.Models;
using Mapster;
using MapsterMapper;

namespace CoffeePeek.JobVacancies.Configuration;

public class MapsterConfiguration
{
    public static IMapper CreateMapper()
    {
        var config = Configure();
        return new Mapper(config);
    }

    private static TypeAdapterConfig Configure()
    {
        var config = new TypeAdapterConfig();

        config.NewConfig<HhVacancyItem, JobVacancy>()
            .Ignore(dest => dest.Id)
            .Map(dest => dest.ExternalId, src => src.Id)
            .Map(dest => dest.Source, _ => "hh.ru")
            .Map(dest => dest.Title, src => src.Name)
            .Map(dest => dest.Company, src => src.Employer != null ? src.Employer.Name : "Unknown")
            .Map(dest => dest.Url, src => src.AlternateUrl)
            .Map(dest => dest.SalaryFrom, src => src.Salary != null ? src.Salary.From : null)
            .Map(dest => dest.SalaryTo, src => src.Salary != null ? src.Salary.To : null)
            .Map(dest => dest.Currency, src => src.Salary != null ? src.Salary.Currency : null)
            .Map(dest => dest.PublishedAt, src => src.PublishedAt)
            .Map(dest => dest.SyncedAt, _ => DateTime.UtcNow)
            .Map(dest => dest.ProfessionalRole,
                src => src.ProfessionalRoles != null && src.ProfessionalRoles.Any()
                    ? src.ProfessionalRoles.First().Name
                    : null)
            .Map(dest => dest.Area, src => src.Area != null ? (src.Area.Name ?? src.Area.Id) : null);
        
        return config;
    }
}