using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hadouken.HadoukenApi.Models
{
    public class SystemInfo
    {
        [JsonProperty(PropertyName = "commitish")]
        public string Commitish { get; set; }

        [JsonProperty(PropertyName = "branch")]
        public string Branch { get; set; }

        [JsonProperty(PropertyName = "versions")]
        public Dictionary<string, string> Versions { get; set; }
    }
}