using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TokenService.Models.ResponseModels;

public class ResponseResult
{
    [JsonProperty("message")]
    [JsonPropertyName("message")]
    public string Message { get; set; } = null!;

    [JsonProperty("content")]
    [JsonPropertyName("content")]
    public Content Content { get; set; } = null!;
}