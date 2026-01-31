namespace BookStore.Application.DTOs;

/// <summary>
/// Standard API error response format for consistent error handling across the application.
/// </summary>
public record ApiError
{
    /// <summary>
    /// A human-readable error message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Optional error code for programmatic error handling.
    /// </summary>
    public string? Code { get; init; }

    /// <summary>
    /// Creates a new ApiError with the specified message.
    /// </summary>
    public static ApiError WithMessage(string message, string? code = null) => new()
    {
        Message = message,
        Code = code
    };

    /// <summary>
    /// Creates a not found error for a resource.
    /// </summary>
    public static ApiError ResourceNotFound(string resourceType, Guid id) => new()
    {
        Message = $"{resourceType} with ID '{id}' not found.",
        Code = "NOT_FOUND"
    };

    /// <summary>
    /// Creates a not found error for a resource relationship.
    /// </summary>
    public static ApiError RelationNotFound(string parentType, Guid parentId, string childType, Guid childId) => new()
    {
        Message = $"{parentType} with ID '{parentId}' or {childType} with ID '{childId}' not found in the {parentType.ToLowerInvariant()}.",
        Code = "NOT_FOUND"
    };
}
