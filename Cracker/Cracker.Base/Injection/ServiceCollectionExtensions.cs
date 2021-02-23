using System;
using System.IO;
using System.Net.Http;
using Cracker.Base.Services;
using Cracker.Base.Settings;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using Refit;
using Serilog;

namespace Cracker.Base.Injection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterAppDirectory(this IServiceCollection services) =>
            services.AddSingleton(new AppFolder(Directory.GetCurrentDirectory()));
        
        public static IServiceCollection RegisterKrakerApi(this IServiceCollection services, Config config)
        {
            services.AddRefitClient<IKrakerApi>()
                .ConfigureHttpClient(client => { client.BaseAddress = new Uri(config.ServerUrl); })
                .AddPolicyHandler(HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .Or<HttpRequestException>()
                    .WaitAndRetryAsync(3,
                        attempt => TimeSpan.FromMilliseconds(300),
                        (ex, span) => Log.Error(ex.Exception.Message)));

            return services;
        }
        
        public static IServiceCollection RegisterConfig(this IServiceCollection services, Config config) 
            => services.AddSingleton(config);

        public static IServiceCollection RegisterAllTypesAsSingleton(this IServiceCollection services) =>
            services
                .Scan(scan => scan.FromAssemblyOf<IKrakerApi>()
                    .AddClasses()
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime()
                );

        public static IServiceCollection RegisterLogging(this IServiceCollection services)
        {
            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine("Logs", "log_{Date}.txt"))
                .CreateLogger();
            
            Log.Logger = logger;
            
            services.AddSingleton<ILogger>(logger);
            return services;
        }
    }
    
    
}