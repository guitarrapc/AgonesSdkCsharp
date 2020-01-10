using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgonesSdk.Hosting.Internals
{
    internal class AgonesSdkConfigurationProvider : ConfigurationProvider
    {
        public AgonesSdkSettings Settings {get;}

        public AgonesSdkConfigurationProvider(AgonesSdkSettings settings)
        {
            Settings = settings;
        }
    }
}
