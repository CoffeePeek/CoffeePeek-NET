using CoffeePeek.BusinessLogic.Configuration;
using CoffeePeek.BusinessLogic.RequestHandlers.CoffeeShop.Moderation.Behavior;
using CoffeePeek.Contract.Requests.CoffeeShop.Review;
using CoffeePeek.Contract.Response.CoffeeShop.Review;
using FluentAssertions;
using MassTransit;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CoffeePeek.BusinessLogic.Tests.Configuration;

public class ConfigurationTests
{
    [Fact]
    public void Configuration_ConfigureBusinessLogic_ShouldRegisterMediatRAndBehaviors()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockPublishEndpoint = new Mock<IPublishEndpoint>();

        services.AddLogging();
        services.AddSingleton(mockPublishEndpoint.Object);

        // Act
        var result = services.ConfigureBusinessLogic();

        // Assert
        result.Should().BeSameAs(services);

        // Check that MediatR is registered
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();
        mediator.Should().NotBeNull();

        // Check that the behavior is registered
        var behavior = serviceProvider
            .GetService<IPipelineBehavior<UpdateModerationCoffeeShopRequest,
                Contract.Response.Response<UpdateModerationCoffeeShopResponse>>>();
        behavior.Should().NotBeNull();
        behavior.Should().BeOfType<UpdateReviewCoffeeShopBehavior>();
    }

    [Fact]
    public void Configuration_ConfigureBusinessLogic_ShouldRegisterServicesAsTransient()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.ConfigureBusinessLogic();

        // Assert
        // Check that the behavior is registered as transient
        var behaviorDescriptor = services.FirstOrDefault(sd =>
            sd.ServiceType ==
            typeof(IPipelineBehavior<UpdateModerationCoffeeShopRequest,
                Contract.Response.Response<UpdateModerationCoffeeShopResponse>>));
        behaviorDescriptor.Should().NotBeNull();
        behaviorDescriptor!.Lifetime.Should().Be(ServiceLifetime.Transient);
    }
}