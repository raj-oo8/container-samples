using Newtonsoft.Json;

namespace AspNet.Grpc.Api.Models
{
    public class Summary
    {
        [JsonProperty("summary")]
        public string? SummaryText { get; set; }
    }
}
