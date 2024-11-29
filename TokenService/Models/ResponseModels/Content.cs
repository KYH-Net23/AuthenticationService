using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TokenService.Models.ResponseModels;

public class Content
{
    [JsonProperty("role")]
    [JsonPropertyName("role")]
    public List<string> Role { get; set; }

    [JsonProperty("dateOfBirth")]
    [JsonPropertyName("dateOfBirth")]
    public DateTime DateOfBirth { get; set; }

    [JsonProperty("streetAddress")]
    [JsonPropertyName("streetAddress")]
    public string StreetAddress { get; set; }

    [JsonProperty("city")]
    [JsonPropertyName("city")]
    public string City { get; set; }

    [JsonProperty("postalCode")]
    [JsonPropertyName("postalCode")]
    public string PostalCode { get; set; }

    [JsonProperty("email")]
    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonProperty("phoneNumber")]
    [JsonPropertyName("phoneNumber")]
    public string PhoneNumber { get; set; }
}