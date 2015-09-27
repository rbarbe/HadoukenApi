using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hadouken.HadoukenApi.Models
{
    // TODO : Map the rest
    public class Properties
    {
        [JsonProperty("trackers")]
        public IList<Uri> Trackers { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("auto_managed")]
        public string AutoManaged { get; set; }
    }
}