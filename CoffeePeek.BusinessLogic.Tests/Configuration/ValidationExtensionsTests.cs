using CoffeePeek.BusinessLogic.Abstractions;
using CoffeePeek.BusinessLogic.Abstractions.Review;
using CoffeePeek.BusinessLogic.Configuration;
using CoffeePeek.Contract.Dtos.User;
using CoffeePeek.Contract.Requests.CoffeeShop;
using CoffeePeek.Infrastructure.Services.Auth.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CoffeePeek.BusinessLogic.Tests.Configuration;

public class ValidationExtensionsTests
{
    [Fact]
    public void ValidationExtensions_AddValidators_ShouldRegisterAllValidationStrategies()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        services.AddSingleton(mockHttpContextAccessor.Object);
        
        // Act
        var result = services.AddValidators();

        // Assert
        result.Should().BeSameAs(services);

        // Check that all expected services are registered
        var serviceProvider = services.BuildServiceProvider();
        
        // Check User validation strategy
        var userValidator = serviceProvider.GetService<IValidationStrategy<UserDto>>();
        userValidator.Should().NotBeNull();
        userValidator.Should().BeOfType<UserCreateValidationStrategy>();

        // Check Review creation validation strategy
        var reviewCreateValidator = serviceProvider.GetService<IValidationStrategy<AddCoffeeShopReviewRequest>>();
        reviewCreateValidator.Should().NotBeNull();
        reviewCreateValidator.Should().BeOfType<ReviewCreateValidationStrategy>();

        // Check Review update validation strategy
        var reviewUpdateValidator = serviceProvider.GetService<IValidationStrategy<UpdateCoffeeShopReviewRequest>>();
        reviewUpdateValidator.Should().NotBeNull();
        reviewUpdateValidator.Should().BeOfType<ReviewUpdateValidationStrategy>();

        // Check User context service
        var userContextService = serviceProvider.GetService<IUserContextService>();
        userContextService.Should().NotBeNull();
    }

    [Fact]
    public void ValidationExtensions_AddValidators_ShouldRegisterServicesAsTransient()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddValidators();

        // Assert
        // Check that services are registered as transient
        var userValidatorDescriptor = services.FirstOrDefault(sd => 
            sd.ServiceType == typeof(IValidationStrategy<UserDto>));
        userValidatorDescriptor.Should().NotBeNull();
        userValidatorDescriptor!.Lifetime.Should().Be(ServiceLifetime.Transient);

        var reviewCreateValidatorDescriptor = services.FirstOrDefault(sd => 
            sd.ServiceType == typeof(IValidationStrategy<AddCoffeeShopReviewRequest>));
        reviewCreateValidatorDescriptor.Should().NotBeNull();
        reviewCreateValidatorDescriptor!.Lifetime.Should().Be(ServiceLifetime.Transient);

        var reviewUpdateValidatorDescriptor = services.FirstOrDefault(sd => 
            sd.ServiceType == typeof(IValidationStrategy<UpdateCoffeeShopReviewRequest>));
        reviewUpdateValidatorDescriptor.Should().NotBeNull();
        reviewUpdateValidatorDescriptor!.Lifetime.Should().Be(ServiceLifetime.Transient);

        var userContextServiceDescriptor = services.FirstOrDefault(sd => 
            sd.ServiceType == typeof(IUserContextService));
        userContextServiceDescriptor.Should().NotBeNull();
        userContextServiceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Transient);
    }
}