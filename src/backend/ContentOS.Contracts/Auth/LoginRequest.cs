namespace ContentOS.Contracts.Auth;

public sealed record LoginRequest(string Email, string Password, bool RememberMe);
