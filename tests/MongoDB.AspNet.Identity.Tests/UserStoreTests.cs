using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using Xunit;

namespace MongoDB.AspNet.Identity.Tests;

[Collection("MongoDB")]
public class UserStoreTests : IAsyncLifetime
{
    private readonly MongoDbFixture _fixture;
    private readonly IMongoDatabase _database;
    private UserStore<IdentityUser> _userStore = null!;

    public UserStoreTests(MongoDbFixture fixture)
    {
        _fixture = fixture;
        _database = fixture.GetDatabase($"TestDb_{Guid.NewGuid():N}");
    }

    public Task InitializeAsync()
    {
        _userStore = new UserStore<IdentityUser>(_database);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _userStore.Dispose();
        await _database.Client.DropDatabaseAsync(_database.DatabaseNamespace.DatabaseName);
    }

    #region IUserStore Tests

    [Fact]
    public async Task CreateAsync_ShouldCreateUser()
    {
        // Arrange
        var user = new IdentityUser("testuser") { Email = "test@example.com" };

        // Act
        var result = await _userStore.CreateAsync(user);

        // Assert
        result.Should().Be(IdentityResult.Success);
        user.Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var user = new IdentityUser("testuser") { Email = "test@example.com" };
        await _userStore.CreateAsync(user);

        // Act
        var foundUser = await _userStore.FindByIdAsync(user.Id);

        // Assert
        foundUser.Should().NotBeNull();
        foundUser!.UserName.Should().Be("testuser");
        foundUser.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Act
        var foundUser = await _userStore.FindByIdAsync("000000000000000000000000");

        // Assert
        foundUser.Should().BeNull();
    }

    [Fact]
    public async Task FindByNameAsync_ShouldReturnUser_WhenNormalizedNameMatches()
    {
        // Arrange
        var user = new IdentityUser("TestUser") { NormalizedUserName = "TESTUSER" };
        await _userStore.CreateAsync(user);

        // Act
        var foundUser = await _userStore.FindByNameAsync("TESTUSER");

        // Assert
        foundUser.Should().NotBeNull();
        foundUser!.UserName.Should().Be("TestUser");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUser()
    {
        // Arrange
        var user = new IdentityUser("testuser") { Email = "old@example.com" };
        await _userStore.CreateAsync(user);

        // Act
        user.Email = "new@example.com";
        var result = await _userStore.UpdateAsync(user);

        // Assert
        result.Should().Be(IdentityResult.Success);
        var foundUser = await _userStore.FindByIdAsync(user.Id);
        foundUser!.Email.Should().Be("new@example.com");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveUser()
    {
        // Arrange
        var user = new IdentityUser("testuser");
        await _userStore.CreateAsync(user);

        // Act
        var result = await _userStore.DeleteAsync(user);

        // Assert
        result.Should().Be(IdentityResult.Success);
        var foundUser = await _userStore.FindByIdAsync(user.Id);
        foundUser.Should().BeNull();
    }

    [Fact]
    public async Task GetUserIdAsync_ShouldReturnUserId()
    {
        // Arrange
        var user = new IdentityUser("testuser");
        await _userStore.CreateAsync(user);

        // Act
        var userId = await _userStore.GetUserIdAsync(user);

        // Assert
        userId.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetUserNameAsync_ShouldReturnUserName()
    {
        // Arrange
        var user = new IdentityUser("testuser");

        // Act
        var userName = await _userStore.GetUserNameAsync(user);

        // Assert
        userName.Should().Be("testuser");
    }

    [Fact]
    public async Task SetUserNameAsync_ShouldUpdateUserName()
    {
        // Arrange
        var user = new IdentityUser("oldname");

        // Act
        await _userStore.SetUserNameAsync(user, "newname");

        // Assert
        user.UserName.Should().Be("newname");
    }

    [Fact]
    public async Task SetNormalizedUserNameAsync_ShouldUpdateNormalizedUserName()
    {
        // Arrange
        var user = new IdentityUser("testuser");

        // Act
        await _userStore.SetNormalizedUserNameAsync(user, "TESTUSER");

        // Assert
        user.NormalizedUserName.Should().Be("TESTUSER");
    }

    #endregion

    #region IUserPasswordStore Tests

    [Fact]
    public async Task SetPasswordHashAsync_ShouldSetPasswordHash()
    {
        // Arrange
        var user = new IdentityUser("testuser");
        var passwordHash = "hashedpassword123";

        // Act
        await _userStore.SetPasswordHashAsync(user, passwordHash);

        // Assert
        user.PasswordHash.Should().Be(passwordHash);
    }

    [Fact]
    public async Task GetPasswordHashAsync_ShouldReturnPasswordHash()
    {
        // Arrange
        var user = new IdentityUser("testuser") { PasswordHash = "hashedpassword123" };

        // Act
        var hash = await _userStore.GetPasswordHashAsync(user);

        // Assert
        hash.Should().Be("hashedpassword123");
    }

    [Fact]
    public async Task HasPasswordAsync_ShouldReturnTrue_WhenPasswordHashExists()
    {
        // Arrange
        var user = new IdentityUser("testuser") { PasswordHash = "hashedpassword123" };

        // Act
        var hasPassword = await _userStore.HasPasswordAsync(user);

        // Assert
        hasPassword.Should().BeTrue();
    }

    [Fact]
    public async Task HasPasswordAsync_ShouldReturnFalse_WhenPasswordHashIsNull()
    {
        // Arrange
        var user = new IdentityUser("testuser") { PasswordHash = null };

        // Act
        var hasPassword = await _userStore.HasPasswordAsync(user);

        // Assert
        hasPassword.Should().BeFalse();
    }

    #endregion

    #region IUserEmailStore Tests

    [Fact]
    public async Task SetEmailAsync_ShouldSetEmail()
    {
        // Arrange
        var user = new IdentityUser("testuser");

        // Act
        await _userStore.SetEmailAsync(user, "test@example.com");

        // Assert
        user.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetEmailAsync_ShouldReturnEmail()
    {
        // Arrange
        var user = new IdentityUser("testuser") { Email = "test@example.com" };

        // Act
        var email = await _userStore.GetEmailAsync(user);

        // Assert
        email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task FindByEmailAsync_ShouldReturnUser_WhenNormalizedEmailMatches()
    {
        // Arrange
        var user = new IdentityUser("testuser")
        {
            Email = "Test@Example.com",
            NormalizedEmail = "TEST@EXAMPLE.COM"
        };
        await _userStore.CreateAsync(user);

        // Act
        var foundUser = await _userStore.FindByEmailAsync("TEST@EXAMPLE.COM");

        // Assert
        foundUser.Should().NotBeNull();
        foundUser!.Email.Should().Be("Test@Example.com");
    }

    [Fact]
    public async Task SetEmailConfirmedAsync_ShouldSetEmailConfirmed()
    {
        // Arrange
        var user = new IdentityUser("testuser");

        // Act
        await _userStore.SetEmailConfirmedAsync(user, true);

        // Assert
        user.EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task GetEmailConfirmedAsync_ShouldReturnEmailConfirmed()
    {
        // Arrange
        var user = new IdentityUser("testuser") { EmailConfirmed = true };

        // Act
        var confirmed = await _userStore.GetEmailConfirmedAsync(user);

        // Assert
        confirmed.Should().BeTrue();
    }

    #endregion

    #region IUserRoleStore Tests

    [Fact]
    public async Task AddToRoleAsync_ShouldAddRole()
    {
        // Arrange
        var user = new IdentityUser("testuser");

        // Act
        await _userStore.AddToRoleAsync(user, "Admin");

        // Assert
        user.Roles.Should().Contain("Admin");
    }

    [Fact]
    public async Task AddToRoleAsync_ShouldNotAddDuplicateRole()
    {
        // Arrange
        var user = new IdentityUser("testuser");
        await _userStore.AddToRoleAsync(user, "Admin");

        // Act
        await _userStore.AddToRoleAsync(user, "admin"); // Case-insensitive

        // Assert
        user.Roles.Should().HaveCount(1);
    }

    [Fact]
    public async Task RemoveFromRoleAsync_ShouldRemoveRole()
    {
        // Arrange
        var user = new IdentityUser("testuser");
        await _userStore.AddToRoleAsync(user, "Admin");

        // Act
        await _userStore.RemoveFromRoleAsync(user, "admin"); // Case-insensitive

        // Assert
        user.Roles.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRolesAsync_ShouldReturnUserRoles()
    {
        // Arrange
        var user = new IdentityUser("testuser");
        await _userStore.AddToRoleAsync(user, "Admin");
        await _userStore.AddToRoleAsync(user, "User");

        // Act
        var roles = await _userStore.GetRolesAsync(user);

        // Assert
        roles.Should().HaveCount(2);
        roles.Should().Contain("Admin");
        roles.Should().Contain("User");
    }

    [Fact]
    public async Task IsInRoleAsync_ShouldReturnTrue_WhenUserHasRole()
    {
        // Arrange
        var user = new IdentityUser("testuser");
        await _userStore.AddToRoleAsync(user, "Admin");

        // Act
        var isInRole = await _userStore.IsInRoleAsync(user, "admin"); // Case-insensitive

        // Assert
        isInRole.Should().BeTrue();
    }

    [Fact]
    public async Task GetUsersInRoleAsync_ShouldReturnUsersWithRole()
    {
        // Arrange
        var user1 = new IdentityUser("user1");
        var user2 = new IdentityUser("user2");
        var user3 = new IdentityUser("user3");

        await _userStore.AddToRoleAsync(user1, "Admin");
        await _userStore.AddToRoleAsync(user2, "Admin");
        await _userStore.AddToRoleAsync(user3, "User");

        await _userStore.CreateAsync(user1);
        await _userStore.CreateAsync(user2);
        await _userStore.CreateAsync(user3);

        // Act
        var admins = await _userStore.GetUsersInRoleAsync("Admin");

        // Assert
        admins.Should().HaveCount(2);
        admins.Select(u => u.UserName).Should().Contain(new[] { "user1", "user2" });
    }

    #endregion

    #region IUserClaimStore Tests

    [Fact]
    public async Task AddClaimsAsync_ShouldAddClaims()
    {
        // Arrange
        var user = new IdentityUser("testuser");
        var claims = new[]
        {
            new Claim("department", "IT"),
            new Claim("level", "senior")
        };

        // Act
        await _userStore.AddClaimsAsync(user, claims);

        // Assert
        user.Claims.Should().HaveCount(2);
        user.Claims.Should().Contain(c => c.ClaimType == "department" && c.ClaimValue == "IT");
        user.Claims.Should().Contain(c => c.ClaimType == "level" && c.ClaimValue == "senior");
    }

    [Fact]
    public async Task GetClaimsAsync_ShouldReturnUserClaims()
    {
        // Arrange
        var user = new IdentityUser("testuser");
        await _userStore.AddClaimsAsync(user, new[] { new Claim("department", "IT") });

        // Act
        var claims = await _userStore.GetClaimsAsync(user);

        // Assert
        claims.Should().HaveCount(1);
        claims.First().Type.Should().Be("department");
        claims.First().Value.Should().Be("IT");
    }

    [Fact]
    public async Task RemoveClaimsAsync_ShouldRemoveClaims()
    {
        // Arrange
        var user = new IdentityUser("testuser");
        var claims = new[] { new Claim("department", "IT"), new Claim("level", "senior") };
        await _userStore.AddClaimsAsync(user, claims);

        // Act
        await _userStore.RemoveClaimsAsync(user, new[] { new Claim("department", "IT") });

        // Assert
        user.Claims.Should().HaveCount(1);
        user.Claims.First().ClaimType.Should().Be("level");
    }

    [Fact]
    public async Task ReplaceClaimAsync_ShouldReplaceClaim()
    {
        // Arrange
        var user = new IdentityUser("testuser");
        await _userStore.AddClaimsAsync(user, new[] { new Claim("department", "IT") });

        // Act
        await _userStore.ReplaceClaimAsync(user, new Claim("department", "IT"), new Claim("department", "HR"));

        // Assert
        user.Claims.Should().HaveCount(1);
        user.Claims.First().ClaimValue.Should().Be("HR");
    }

    [Fact]
    public async Task GetUsersForClaimAsync_ShouldReturnUsersWithClaim()
    {
        // Arrange
        var user1 = new IdentityUser("user1");
        var user2 = new IdentityUser("user2");
        var user3 = new IdentityUser("user3");

        await _userStore.AddClaimsAsync(user1, new[] { new Claim("department", "IT") });
        await _userStore.AddClaimsAsync(user2, new[] { new Claim("department", "IT") });
        await _userStore.AddClaimsAsync(user3, new[] { new Claim("department", "HR") });

        await _userStore.CreateAsync(user1);
        await _userStore.CreateAsync(user2);
        await _userStore.CreateAsync(user3);

        // Act
        var users = await _userStore.GetUsersForClaimAsync(new Claim("department", "IT"));

        // Assert
        users.Should().HaveCount(2);
        users.Select(u => u.UserName).Should().Contain(new[] { "user1", "user2" });
    }

    #endregion

    #region IUserLoginStore Tests

    [Fact]
    public async Task AddLoginAsync_ShouldAddLogin()
    {
        // Arrange
        var user = new IdentityUser("testuser");
        var login = new UserLoginInfo("Google", "google-id-123", "Google");

        // Act
        await _userStore.AddLoginAsync(user, login);

        // Assert
        user.Logins.Should().HaveCount(1);
        user.Logins.First().LoginProvider.Should().Be("Google");
        user.Logins.First().ProviderKey.Should().Be("google-id-123");
    }

    [Fact]
    public async Task RemoveLoginAsync_ShouldRemoveLogin()
    {
        // Arrange
        var user = new IdentityUser("testuser");
        await _userStore.AddLoginAsync(user, new UserLoginInfo("Google", "google-id-123", "Google"));

        // Act
        await _userStore.RemoveLoginAsync(user, "Google", "google-id-123");

        // Assert
        user.Logins.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLoginsAsync_ShouldReturnUserLogins()
    {
        // Arrange
        var user = new IdentityUser("testuser");
        await _userStore.AddLoginAsync(user, new UserLoginInfo("Google", "google-id-123", "Google"));
        await _userStore.AddLoginAsync(user, new UserLoginInfo("Facebook", "fb-id-456", "Facebook"));

        // Act
        var logins = await _userStore.GetLoginsAsync(user);

        // Assert
        logins.Should().HaveCount(2);
    }

    [Fact]
    public async Task FindByLoginAsync_ShouldReturnUser_WhenLoginExists()
    {
        // Arrange
        var user = new IdentityUser("testuser");
        await _userStore.AddLoginAsync(user, new UserLoginInfo("Google", "google-id-123", "Google"));
        await _userStore.CreateAsync(user);

        // Act
        var foundUser = await _userStore.FindByLoginAsync("Google", "google-id-123");

        // Assert
        foundUser.Should().NotBeNull();
        foundUser!.UserName.Should().Be("testuser");
    }

    #endregion

    #region IUserLockoutStore Tests

    [Fact]
    public async Task SetLockoutEndDateAsync_ShouldSetLockoutEnd()
    {
        // Arrange
        var user = new IdentityUser("testuser");
        var lockoutEnd = DateTimeOffset.UtcNow.AddHours(1);

        // Act
        await _userStore.SetLockoutEndDateAsync(user, lockoutEnd);

        // Assert
        user.LockoutEnd.Should().BeCloseTo(lockoutEnd, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetLockoutEndDateAsync_ShouldReturnLockoutEnd()
    {
        // Arrange
        var lockoutEnd = DateTimeOffset.UtcNow.AddHours(1);
        var user = new IdentityUser("testuser") { LockoutEnd = lockoutEnd };

        // Act
        var result = await _userStore.GetLockoutEndDateAsync(user);

        // Assert
        result.Should().BeCloseTo(lockoutEnd, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task IncrementAccessFailedCountAsync_ShouldIncrementCount()
    {
        // Arrange
        var user = new IdentityUser("testuser") { AccessFailedCount = 2 };

        // Act
        var count = await _userStore.IncrementAccessFailedCountAsync(user);

        // Assert
        count.Should().Be(3);
        user.AccessFailedCount.Should().Be(3);
    }

    [Fact]
    public async Task ResetAccessFailedCountAsync_ShouldResetCount()
    {
        // Arrange
        var user = new IdentityUser("testuser") { AccessFailedCount = 5 };

        // Act
        await _userStore.ResetAccessFailedCountAsync(user);

        // Assert
        user.AccessFailedCount.Should().Be(0);
    }

    [Fact]
    public async Task SetLockoutEnabledAsync_ShouldSetLockoutEnabled()
    {
        // Arrange
        var user = new IdentityUser("testuser");

        // Act
        await _userStore.SetLockoutEnabledAsync(user, true);

        // Assert
        user.LockoutEnabled.Should().BeTrue();
    }

    #endregion

    #region IUserTwoFactorStore Tests

    [Fact]
    public async Task SetTwoFactorEnabledAsync_ShouldSetTwoFactorEnabled()
    {
        // Arrange
        var user = new IdentityUser("testuser");

        // Act
        await _userStore.SetTwoFactorEnabledAsync(user, true);

        // Assert
        user.TwoFactorEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task GetTwoFactorEnabledAsync_ShouldReturnTwoFactorEnabled()
    {
        // Arrange
        var user = new IdentityUser("testuser") { TwoFactorEnabled = true };

        // Act
        var enabled = await _userStore.GetTwoFactorEnabledAsync(user);

        // Assert
        enabled.Should().BeTrue();
    }

    #endregion

    #region IUserSecurityStampStore Tests

    [Fact]
    public async Task SetSecurityStampAsync_ShouldSetSecurityStamp()
    {
        // Arrange
        var user = new IdentityUser("testuser");
        var stamp = Guid.NewGuid().ToString();

        // Act
        await _userStore.SetSecurityStampAsync(user, stamp);

        // Assert
        user.SecurityStamp.Should().Be(stamp);
    }

    [Fact]
    public async Task GetSecurityStampAsync_ShouldReturnSecurityStamp()
    {
        // Arrange
        var stamp = Guid.NewGuid().ToString();
        var user = new IdentityUser("testuser") { SecurityStamp = stamp };

        // Act
        var result = await _userStore.GetSecurityStampAsync(user);

        // Assert
        result.Should().Be(stamp);
    }

    #endregion

    #region IUserPhoneNumberStore Tests

    [Fact]
    public async Task SetPhoneNumberAsync_ShouldSetPhoneNumber()
    {
        // Arrange
        var user = new IdentityUser("testuser");

        // Act
        await _userStore.SetPhoneNumberAsync(user, "+1234567890");

        // Assert
        user.PhoneNumber.Should().Be("+1234567890");
    }

    [Fact]
    public async Task GetPhoneNumberAsync_ShouldReturnPhoneNumber()
    {
        // Arrange
        var user = new IdentityUser("testuser") { PhoneNumber = "+1234567890" };

        // Act
        var phone = await _userStore.GetPhoneNumberAsync(user);

        // Assert
        phone.Should().Be("+1234567890");
    }

    [Fact]
    public async Task SetPhoneNumberConfirmedAsync_ShouldSetPhoneNumberConfirmed()
    {
        // Arrange
        var user = new IdentityUser("testuser");

        // Act
        await _userStore.SetPhoneNumberConfirmedAsync(user, true);

        // Assert
        user.PhoneNumberConfirmed.Should().BeTrue();
    }

    #endregion

    #region IUserAuthenticationTokenStore Tests

    [Fact]
    public async Task SetTokenAsync_ShouldSetToken()
    {
        // Arrange
        var user = new IdentityUser("testuser");

        // Act
        await _userStore.SetTokenAsync(user, "Google", "access_token", "token-value-123");

        // Assert
        user.Tokens.Should().HaveCount(1);
        user.Tokens.First().LoginProvider.Should().Be("Google");
        user.Tokens.First().Name.Should().Be("access_token");
        user.Tokens.First().Value.Should().Be("token-value-123");
    }

    [Fact]
    public async Task GetTokenAsync_ShouldReturnToken()
    {
        // Arrange
        var user = new IdentityUser("testuser");
        await _userStore.SetTokenAsync(user, "Google", "access_token", "token-value-123");

        // Act
        var token = await _userStore.GetTokenAsync(user, "Google", "access_token");

        // Assert
        token.Should().Be("token-value-123");
    }

    [Fact]
    public async Task RemoveTokenAsync_ShouldRemoveToken()
    {
        // Arrange
        var user = new IdentityUser("testuser");
        await _userStore.SetTokenAsync(user, "Google", "access_token", "token-value-123");

        // Act
        await _userStore.RemoveTokenAsync(user, "Google", "access_token");

        // Assert
        user.Tokens.Should().BeEmpty();
    }

    #endregion

    #region IQueryableUserStore Tests

    [Fact]
    public async Task Users_ShouldReturnQueryableUsers()
    {
        // Arrange
        await _userStore.CreateAsync(new IdentityUser("user1"));
        await _userStore.CreateAsync(new IdentityUser("user2"));
        await _userStore.CreateAsync(new IdentityUser("user3"));

        // Act
        var users = _userStore.Users.ToList();

        // Assert
        users.Should().HaveCount(3);
    }

    #endregion
}
