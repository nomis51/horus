using System.Text.Json.Serialization;

namespace Horus.Shared.Models.GitHub;

public class Release
{
    [JsonPropertyName("tag_name")]
    public string TagName { get; set; }
}