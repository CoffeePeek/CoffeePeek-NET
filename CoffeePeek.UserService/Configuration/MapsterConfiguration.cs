using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.UserService.Models;
using Mapster;
using MapsterMapper;

namespace CoffeePeek.UserService.Configuration;

public static class MapsterConfiguration
{
    public static IMapper CreateMapper()
    {
        var config = Configure();
        return new Mapper(config);
    }

    private static TypeAdapterConfig Configure()
    {
        var config = new TypeAdapterConfig();

        config.NewConfig<User, UserDto>()
            .Map(dest => dest.PhotoUrl, src => src.AvatarUrl)
            .Map(dest => dest.ReviewCount, src => src.UserStatistics.ReviewCount)
            .Map(dest => dest.CheckInCount, src => src.UserStatistics.CheckInCount)
            .Map(dest => dest.AddedShopsCount, src => src.UserStatistics.AddedShopsCount);
            
        return config;
    }
}