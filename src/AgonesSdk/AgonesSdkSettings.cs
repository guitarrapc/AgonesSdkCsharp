using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AgonesSdk
{
    public class AgonesSdkSettings
    {
        public string HttpClientName { get; set; } = "Agones";
        public bool CacheRequest { get; set; } = true;
        public TimeSpan HealthInterval { get; set; } = TimeSpan.FromSeconds(2);
    }
}
