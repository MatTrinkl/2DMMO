using Mmo.Server.ServiceAuthentication;
using Mmo.Server.ServiceAuthentication.Records;
using Mmo.Server.Tests.Helpers;
using Mmo.Shared.Account.Enums;
using Mmo.Shared.Enums;

namespace Mmo.Server.Tests.Services;

public class AuthenticationServiceTests
{
    private readonly MockLog _mockLog = new();

    [Fact]
    public async Task AuthenticateAsync_ValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var authService = new AuthenticationService(_mockLog);

        // Act
        AuthResult result = await authService.AuthenticateAsync("TestUser", "password123");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.AccountId);
        Assert.NotEqual(Guid.Empty, result.AccountId.Value);
        Assert.Equal("TestUser", result.Username);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task AuthenticateAsync_AnyUsername_LogsAuthentication()
    {
        // Arrange
        var authService = new AuthenticationService(_mockLog);

        // Act
        await authService.AuthenticateAsync("AnyUser", "anyPassword");

        // Assert
        Assert.True(_mockLog.HasMessageContaining("INFO", "Authentication successful"));
    }

    [Fact]
    public async Task AuthenticateAsync_DifferentUsers_ReturnDifferentAccountIds()
    {
        // Arrange
        var authService = new AuthenticationService(_mockLog);

        // Act
        AuthResult result1 = await authService.AuthenticateAsync("User1", "pass1");
        AuthResult result2 = await authService.AuthenticateAsync("User2", "pass2");

        // Assert
        Assert.NotEqual(result1.AccountId, result2.AccountId);
    }

    [Fact]
    public async Task ValidateSessionAsync_ReturnsNotImplemented()
    {
        // Arrange
        var authService = new AuthenticationService(_mockLog);
        var sessionToken = Guid.NewGuid();

        // Act
        AuthResult result = await authService.ValidateSessionAsync(sessionToken);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Session validation not implemented", result.Error);
    }

    [Fact]
    public async Task InvalidateSessionAsync_LogsSessionInvalidation()
    {
        // Arrange
        var authService = new AuthenticationService(_mockLog);
        var sessionToken = Guid.NewGuid();

        // Act
        await authService.InvalidateSessionAsync(sessionToken);

        // Assert
        Assert.True(_mockLog.HasMessageContaining("INFO", "Session invalidated"));
    }

    [Fact]
    public async Task AuthenticateAsync_DefaultFlags_ReturnsNone()
    {
        // Arrange
        var authService = new AuthenticationService(_mockLog);

        // Act
        AuthResult result = await authService.AuthenticateAsync("TestUser", "password");

        // Assert
        Assert.Equal(AccountFlags.None, result.Flags);
    }
}
