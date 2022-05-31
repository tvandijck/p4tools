using System.Text.Json.Serialization;

namespace p4tools
{
    public class PerforceChange
    {
        [JsonPropertyName("change")]
        public string? CL { get; set; }

        [JsonPropertyName("changeType")]
        public string? ChangeType { get; set; }

        [JsonPropertyName("client")]
        public string? Client { get; set; }

        [JsonPropertyName("desc")]
        public string? Description { get; set; }

        [JsonPropertyName("shelved")]
        public string? Shelved { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("time")]
        public string? Time { get; set; }

        [JsonPropertyName("user")]
        public string? User { get; set; }
    }
}