using System.Text.Json.Serialization;

namespace BookStore.Infrastructure.External;

/// <summary>
/// DTO for deserializing book data from the external Potter API.
/// </summary>
internal class ExternalBookDto
{
    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("originalTitle")]
    public string OriginalTitle { get; set; } = string.Empty;

    [JsonPropertyName("releaseDate")]
    public string ReleaseDate { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("pages")]
    public int Pages { get; set; }

    [JsonPropertyName("cover")]
    public string Cover { get; set; } = string.Empty;

    [JsonPropertyName("index")]
    public int Index { get; set; }
}
