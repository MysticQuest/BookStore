using System.ComponentModel.DataAnnotations;

namespace BookStore.Application.Options;

/// <summary>
/// Configuration settings for the book fetch service.
/// </summary>
public class BookFetchSettings
{
    /// <summary>
    /// The configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "BookFetchSettings";

    /// <summary>
    /// The URL of the external API to fetch books from.
    /// </summary>
    [Required]
    [Url]
    public string Url { get; set; } = string.Empty;
}
