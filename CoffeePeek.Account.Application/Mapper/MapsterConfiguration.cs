using CoffeePeek.Account.Application.Features.User.GetProfile;
using CoffeePeek.Account.Domain.Entities.UserAggregate;
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

        config.NewConfig<User, UserProfileResponse>()
            .Map(d => d.UserName, s => s.Username.Value)
            .Map(d => d.Email, s => s.Credentials.Email)
            .Map(dest => dest.ReviewCount, src => src.Statistics.ReviewCount)
            .Map(dest => dest.CheckInCount, src => src.Statistics.CheckInCount)
            .Map(dest => dest.AddedShopsCount, src => src.Statistics.AddedShopsCount)
            .Map(dest => dest.AvatarUrl, src => src.PhotoMetadata != null ? $"https://bucket-dev-771f.up.railway.app/coffee.avatars/{src.PhotoMetadata!.StorageKey}" : null);
        
        return config;
    }
}