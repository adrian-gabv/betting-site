namespace BettingSite.Application.Common;

public record PhotoUploadResult
{
    public string? Error { get; init; }
    public string Url { get; init; } = string.Empty;
    public string? PublicId { get; init; }
}

public record PhotoDeleteResult
{
    public string? Error { get; init; }
}
