using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgonesSdk.Internals
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
