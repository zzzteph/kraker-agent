using System;
using Cracker.Base.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Cracker.Base.Injection
{
    public static class ServiceProviderBuilder
    {
        public static IServiceProvider Build(Config config) =>
            new ServiceCollection()
                .RegisterAppDirectory()
                .RegisterConfig(config)
                .RegisterLogging()
                .RegisterKrakerApi(config)
                .RegisterAllTypesAsSingleton()
                .BuildServiceProvider();
    }
}