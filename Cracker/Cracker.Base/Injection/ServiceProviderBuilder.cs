using System;
using Microsoft.Extensions.DependencyInjection;

namespace Cracker.Base.Injection
{
    public static class ServiceProviderBuilder
    {
        public static IServiceProvider Build() =>
            new ServiceCollection()
                .RegisterAllTypesAsSingleton()
                .BuildServiceProvider();
    }
}