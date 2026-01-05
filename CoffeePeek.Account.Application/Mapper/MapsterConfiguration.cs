using CoffeePeek.Account.Domain.Aggregates;
using CoffeePeek.Account.Domain.Aggregates.UserAggregate;
using CoffeePeek.Contract.Dtos.User;
using Mapster;
using MapsterMapper;

namespace CoffeePeek.Account.Application.Mapper;

public static class MapsterConfiguration
{
    public static IMapper CreateMapper()
    {
        var config = Configure();
        return new MapsterMapper.Mapper(config);
    }

    private static TypeAdapterConfig Configure()
    {
        var config = new TypeAdapterConfig();

        TypeAdapterConfig<PhotoMetadata, string>.NewConfig()
            .MapWith(src => $"https://bucket-dev-771f.up.railway.app/coffee.avatars/{src.StorageKey}");
        
        config.NewConfig<User, UserDto>()
            .Map(d => d.Email, s => s.UserCredential.Email)
            .Map(dest => dest.ReviewCount, src => src.UserStatistics.ReviewCount)
            .Map(dest => dest.CheckInCount, src => src.UserStatistics.CheckInCount)
            .Map(dest => dest.AddedShopsCount, src => src.UserStatistics.AddedShopsCount)
            .Map(dest => dest.AvatarUrl, src => src.PhotoMetadata);
            
        return config;
    }
}