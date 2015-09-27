using Newtonsoft.Json;

namespace Hadouken.HadoukenApi.Models.Settings
{
    // TODO : Map the rest
    public class Settings
    {
        [JsonProperty("bind_port")]
        public long BindPort { get; set; }

        public long ConnectionsLimit { get; set; }

        [JsonProperty("dht")]
        public bool Dht { get; set; }

        public string[] DownloadDirectories { get; set; }
        public bool BwManagement { get; set; }
        public long MaximumDownloadRate { get; set; }
        public bool RateLimitIpOverhead { get; set; }
        public bool RateLimitUtp { get; set; }
        public string AbsoluteSavePath { get; set; }
        public bool Lsd { get; set; }
        public bool Natpmp { get; set; }
        public bool Upnp { get; set; }
        public string Cookie { get; set; }
        public bool AllowSameIp { get; set; }
        public bool AnonymousMode { get; set; }
        public bool Utp { get; set; }
        public long HalfOpenLimit { get; set; }
    }
}