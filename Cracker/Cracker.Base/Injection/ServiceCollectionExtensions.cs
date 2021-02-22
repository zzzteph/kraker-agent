using Cracker.Base.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cracker.Base.Injection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterAllTypesAsSingleton(this IServiceCollection services) =>
            services
                .Scan(scan => scan.FromAssemblyOf<IKrakerApi>()
                    .AddClasses()
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime()
                );
    }
}