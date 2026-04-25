using Autofac;
using CoffeePeek.Client.App.Services;

namespace CoffeePeek.Client.App.Configuration;

public class UiServiceModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ImagePickerService>()
            .As<IImagePickerService>()
            .SingleInstance();
    }
}