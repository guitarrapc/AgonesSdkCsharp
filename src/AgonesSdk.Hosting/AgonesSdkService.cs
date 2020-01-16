using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace AgonesSdk.Hosting
{
    public static class AgonesSdkService
    {
        private static readonly Lazy<Random> jitterer = new Lazy<Random>(() => new Random());
        private static readonly Lazy<AgonesSdkSettings> defaultSettings = new Lazy<AgonesSdkSettings>(() => new AgonesSdkSettings());

        public static IHostBuilder AddAgonesSdk(this IHostBuilder hostBuilder, bool registerHostedService = true)
        {
            return hostBuilder.AddAgonesSdk(defaultSettings.Value, registerHostedService);
        }
        public static IHostBuilder AddAgonesSdk(this IHostBuilder hostBuilder, AgonesSdkSettings settings, bool registerHostedService = true)
        {
            static void configureDefaultHttpClient(IServiceCollection services, AgonesSdkSettings settings)
            {
                services.AddHttpClient(settings.HttpClientName, client => client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")))
                            .AddTransientHttpErrorPolicy(x => x.WaitAndRetryAsync(3, retry => ExponentialBackkoff(retry)))
                            .AddTransientHttpErrorPolicy(x => x.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));
            }

            return hostBuilder.ConfigureServices((hostContext, services) =>
            {
                ConfigureAgonesService(services, settings, configureDefaultHttpClient, registerHostedService);
            });
        }
        public static IHostBuilder AddAgonesSdk(this IHostBuilder hostBuilder, AgonesSdkSettings settings, Action<IServiceCollection, AgonesSdkSettings> configureHttpClient, bool registerHostedService = true)
        {
            return hostBuilder.ConfigureServices((hostContext, services) =>
            {
                ConfigureAgonesService(services, settings, configureHttpClient, registerHostedService);
            });
        }

        private static void ConfigureAgonesService(IServiceCollection services, AgonesSdkSettings settings, Action<IServiceCollection, AgonesSdkSettings> configureHttpClient, bool registerHostedService)
        {
            configureHttpClient.Invoke(services, settings);

            services.AddSingleton<AgonesSdkSettings>(settings);
            services.AddSingleton<IAgonesSdk, AgonesSdk>();
            if (registerHostedService)
            {
                services.AddHostedService<AgonesHostedService>();
            }
        }

        public static TimeSpan ExponentialBackkoff(int retryAttempt)
            => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Value.Next(0, 100));
    }
}
