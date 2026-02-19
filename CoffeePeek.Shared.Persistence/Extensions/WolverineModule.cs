using System.Reflection;
using JasperFx.CodeGeneration;
using Microsoft.Extensions.Hosting;
using Wolverine;

namespace CoffeePeek.Shared.Persistence.Extensions;

public static class WolverineModule
{
    extension(IHostBuilder hostBuilder)
    {
        public IHostBuilder AddWolverine(Assembly handlerAssembly)
        {
            hostBuilder.UseWolverine(opts =>
            {
                opts.CodeGeneration.TypeLoadMode = TypeLoadMode.Dynamic;
                opts.Discovery.IncludeAssembly(handlerAssembly);
            });

            return hostBuilder;
        }
    }
}