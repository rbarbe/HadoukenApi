using Newtonsoft.Json;

namespace Hadouken.HadoukenApi.Models
{
    public class JsonrpcRequest
    {
        [JsonProperty("jsonrpc")]
        public string Jsonrpc { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("params")]
        public object[] Params { get; set; }
    }
}