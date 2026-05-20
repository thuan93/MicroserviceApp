namespace Auth.Api.Models;

public record LoginResponse(string Token, DateTime ExpiresAt);
