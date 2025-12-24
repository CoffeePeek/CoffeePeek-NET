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

        config.NewConfig<Account.Domain.Entities.User, UserDto>()
            .Map(dest => dest.Roles, src => src.UserCredential.UserRoles.Select(x => x.Role.Name))
            .Map(dest => dest.PhotoUrl, src => src.AvatarUrl)
            .Map(dest => dest.ReviewCount, src => src.UserStatistics.ReviewCount)
            .Map(dest => dest.CheckInCount, src => src.UserStatistics.CheckInCount)
            .Map(dest => dest.AddedShopsCount, src => src.UserStatistics.AddedShopsCount);
            
        return config;
    }
}