using System.ComponentModel.DataAnnotations;

namespace BookStore.Client.ViewModels;

/// <summary>
/// View model for displaying and editing book information.
/// </summary>
public class BookViewModel
{
    private const string Unavailable = "Unavailable";

    public Guid Id { get; set; }

    public int Number { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(500, ErrorMessage = "Title cannot exceed 500 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Original title cannot exceed 500 characters.")]
    public string OriginalTitle { get; set; } = string.Empty;

    public DateTime? ReleaseDate { get; set; }

    [StringLength(4000, ErrorMessage = "Description cannot exceed 4000 characters.")]
    public string Description { get; set; } = string.Empty;

    [Range(0, 10000, ErrorMessage = "Pages must be between 0 and 10,000.")]
    public int Pages { get; set; }

    [Url(ErrorMessage = "Cover must be a valid URL.")]
    public string Cover { get; set; } = string.Empty;

    public int Index { get; set; }

    [Required(ErrorMessage = "Number of copies is required.")]
    [Range(0, 100000, ErrorMessage = "Number of copies must be between 0 and 100,000.")]
    public int NumberOfCopies { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    [Range(0, 9999.99, ErrorMessage = "Price must be between €0.00 and €9,999.99.")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    #region Display Properties (with fallbacks for invalid/missing data)

    /// <summary>
    /// Display-friendly title with fallback for empty values.
    /// </summary>
    public string DisplayTitle => string.IsNullOrWhiteSpace(Title) ? Unavailable : Title;

    /// <summary>
    /// Display-friendly original title with fallback for empty values.
    /// </summary>
    public string DisplayOriginalTitle => string.IsNullOrWhiteSpace(OriginalTitle) ? Unavailable : OriginalTitle;

    /// <summary>
    /// Display-friendly release date with fallback for empty values.
    /// </summary>
    public string DisplayReleaseDate => ReleaseDate.HasValue ? ReleaseDate.Value.ToString("MMMM d, yyyy") : Unavailable;

    /// <summary>
    /// Display-friendly description with fallback for empty values.
    /// </summary>
    public string DisplayDescription => string.IsNullOrWhiteSpace(Description) ? Unavailable : Description;

    /// <summary>
    /// Display-friendly page count with fallback for invalid values.
    /// </summary>
    public string DisplayPages => Pages > 0 ? Pages.ToString() : Unavailable;

    /// <summary>
    /// Display-friendly cover URL with fallback for empty values.
    /// </summary>
    public string DisplayCover => string.IsNullOrWhiteSpace(Cover) ? string.Empty : Cover;

    /// <summary>
    /// Indicates whether the cover image is available.
    /// </summary>
    public bool HasCover => !string.IsNullOrWhiteSpace(Cover);

    /// <summary>
    /// Display-friendly price formatted as currency.
    /// </summary>
    public string DisplayPrice => Price >= 0 ? $"€{Price:F2}" : Unavailable;

    /// <summary>
    /// Display-friendly number of copies with fallback for negative values.
    /// </summary>
    public string DisplayNumberOfCopies => NumberOfCopies >= 0 ? NumberOfCopies.ToString() : Unavailable;

    /// <summary>
    /// Indicates whether this book has valid core data.
    /// </summary>
    public bool IsValid => !string.IsNullOrWhiteSpace(Title) && Number > 0;

    #endregion
}
