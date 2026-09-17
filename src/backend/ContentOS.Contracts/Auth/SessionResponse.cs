namespace ContentOS.Contracts.Auth;

public sealed record SessionResponse(bool IsAuthenticated, string? UserName);
