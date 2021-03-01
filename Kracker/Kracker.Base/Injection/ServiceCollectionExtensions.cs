using System;
using System.IO;
using System.Net.Http;
using Kracker.Base.Domain.Configuration;
using Kracker.Base.Domain.Folders;
using Kracker.Base.Domain.Jobs;
using Kracker.Base.Services;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Refit;
using Serilog;

namespace Kracker.Base.Injection
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
                        (ex, span) => Log.Error(ex?.Exception?.Message)));

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
                )
                .AddTransient<IAgent, Agent>();

        public static IServiceCollection RegisterLogging(this IServiceCollection services)
        {
            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.RollingFile(Path.Combine("Logs", "log_{Date}.txt"), retainedFileCountLimit: 7)
                .CreateLogger();
            
            Log.Logger = logger;
            
            services.AddSingleton<ILogger>(logger);
            return services;
        }
    }
    
    
}