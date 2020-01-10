using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgonesSdk.Hosting.Internals
{
    internal class AgonesSdkConfigurationSource : IConfigurationSource
    {
        public AgonesSdkSettings Settings { get; }
        public AgonesSdkConfigurationSource(AgonesSdkSettings settings)
        {
            Settings = settings;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new AgonesSdkConfigurationProvider(Settings);
        }
    }
}
