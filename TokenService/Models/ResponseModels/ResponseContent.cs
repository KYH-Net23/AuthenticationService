using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TokenService.Models.ResponseModels;

public class ResponseContent
{
    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonProperty("email")]
    [JsonPropertyName("email")]
    public string Email { get; set; } = null!;

    [JsonProperty("roles")]
    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; } = null!;
}