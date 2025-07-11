using System.Text.Json.Serialization;

namespace Crosscutting.DTOs.MercadoPago
{
    public class MercadoPagoWebhookModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("live_mode")]
        public bool LiveMode { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("date_created")]
        public DateTime DateCreated { get; set; }

        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("api_version")]
        public string ApiVersion { get; set; }

        [JsonPropertyName("action")]
        public string Action { get; set; }

        [JsonPropertyName("data")]
        public Data Data { get; set; }

    }

    public class Data
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// approved | cancelled
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } 
        
        public string external_reference { get; set; }
    }

}
