using System.ComponentModel.DataAnnotations;

namespace BookStore.Application.Options;

/// <summary>
/// Configuration settings for CORS.
/// </summary>
public class CorsSettings
{
    /// <summary>
    /// The configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Cors";

    /// <summary>
    /// The list of allowed origins for CORS.
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "At least one CORS origin must be configured.")]
    public string[] AllowedOrigins { get; set; } = [];
}
