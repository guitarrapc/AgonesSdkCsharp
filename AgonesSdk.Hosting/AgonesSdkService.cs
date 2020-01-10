using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgonesSdk
{
    public class AgonesSdkService
    {
        public static IHostedService AddAgonesSdk(this IHostBuilder hostBuilder, string configurationName = "",)
        {
            hostBuilder.ConfigureServices((hostContext, services) => 
            {
                services.AddSingleton<IAgonesSdk>(serviceProvider =>
                {
                    
                })
            }
        }
    }
}
