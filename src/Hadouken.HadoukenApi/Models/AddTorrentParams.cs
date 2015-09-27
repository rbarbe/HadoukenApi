using Newtonsoft.Json;

namespace Hadouken.HadoukenApi.Models
{
    public class AddTorrentParams
    {
        public AddTorrentParams()
        {
            SavePath = "0";
            SubPath = "";
        }

        [JsonProperty("savePath")]
        public string SavePath { get; set; }

        [JsonProperty("subPath")]
        public string SubPath { get; set; }

        [JsonProperty("paused")]
        public bool Paused { get; set; }

        [JsonProperty("sequentialDownload")]
        public bool SequentialDownload { get; set; }
    }
}