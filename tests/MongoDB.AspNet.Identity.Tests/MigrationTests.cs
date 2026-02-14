using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace MongoDB.AspNet.Identity.Tests;

[Collection("MongoDB")]
public class MigrationTests : IAsyncLifetime
{
    private readonly MongoDbFixture _fixture;
    private readonly IMongoDatabase _database;

    public MigrationTests(MongoDbFixture fixture)
    {
        _fixture = fixture;
        _database = fixture.GetDatabase($"MigrationTestDb_{Guid.NewGuid():N}");
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _database.Client.DropDatabaseAsync(_database.DatabaseNamespace.DatabaseName);
    }

    [Fact]
    public async Task Migration_ShouldAddNormalizedUserName_WhenMissing()
    {
        // Arrange - Insert a legacy user document without NormalizedUserName
        var usersCollection = _database.GetCollection<BsonDocument>("AspNetUsers");
        var legacyUser = new BsonDocument
        {
            { "_id", ObjectId.GenerateNewId() },
            { "UserName", "TestUser" },
            { "Email", "test@example.com" },
            { "PasswordHash", "somehash" },
            { "SecurityStamp", Guid.NewGuid().ToString() },
            { "Roles", new BsonArray() },
            { "Claims", new BsonArray() },
            { "Logins", new BsonArray() }
        };
        await usersCollection.InsertOneAsync(legacyUser);

        // Reset migration marker to force re-migration
        await MigrationHelper.ResetMigrationAsync(_database);

        // Act - Create UserStore which triggers migration
        using var userStore = new UserStore<IdentityUser>(_database);
        var foundUser = await userStore.FindByNameAsync("TESTUSER");

        // Assert
        foundUser.Should().NotBeNull();
        foundUser!.UserName.Should().Be("TestUser");
        foundUser.NormalizedUserName.Should().Be("TESTUSER");
    }

    [Fact]
    public async Task Migration_ShouldAddNormalizedEmail_WhenMissing()
    {
        // Arrange - Insert a legacy user document without NormalizedEmail
        var usersCollection = _database.GetCollection<BsonDocument>("AspNetUsers");
        var legacyUser = new BsonDocument
        {
            { "_id", ObjectId.GenerateNewId() },
            { "UserName", "TestUser2" },
            { "Email", "Test2@Example.com" },
            { "PasswordHash", "somehash" },
            { "SecurityStamp", Guid.NewGuid().ToString() },
            { "Roles", new BsonArray() },
            { "Claims", new BsonArray() },
            { "Logins", new BsonArray() }
        };
        await usersCollection.InsertOneAsync(legacyUser);

        // Reset migration marker to force re-migration
        await MigrationHelper.ResetMigrationAsync(_database);

        // Act - Create UserStore which triggers migration
        using var userStore = new UserStore<IdentityUser>(_database);
        var foundUser = await userStore.FindByEmailAsync("TEST2@EXAMPLE.COM");

        // Assert
        foundUser.Should().NotBeNull();
        foundUser!.Email.Should().Be("Test2@Example.com");
        foundUser.NormalizedEmail.Should().Be("TEST2@EXAMPLE.COM");
    }

    [Fact]
    public async Task Migration_ShouldConvertLockoutEndDateUtc_ToLockoutEnd()
    {
        // Arrange - Insert a legacy user document with LockoutEndDateUtc
        var usersCollection = _database.GetCollection<BsonDocument>("AspNetUsers");
        var lockoutDate = DateTime.UtcNow.AddHours(1);
        var legacyUser = new BsonDocument
        {
            { "_id", ObjectId.GenerateNewId() },
            { "UserName", "LockedUser" },
            { "Email", "locked@example.com" },
            { "LockoutEndDateUtc", lockoutDate },
            { "LockoutEnabled", true },
            { "Roles", new BsonArray() },
            { "Claims", new BsonArray() },
            { "Logins", new BsonArray() }
        };
        await usersCollection.InsertOneAsync(legacyUser);

        // Reset migration marker to force re-migration
        await MigrationHelper.ResetMigrationAsync(_database);

        // Act - Create UserStore which triggers migration
        using var userStore = new UserStore<IdentityUser>(_database);
        var foundUser = await userStore.FindByNameAsync("LOCKEDUSER");

        // Assert
        foundUser.Should().NotBeNull();
        foundUser!.LockoutEnd.Should().NotBeNull();
        foundUser.LockoutEnd!.Value.UtcDateTime.Should().BeCloseTo(lockoutDate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Migration_ShouldAddConcurrencyStamp_WhenMissing()
    {
        // Arrange - Insert a legacy user document without ConcurrencyStamp
        var usersCollection = _database.GetCollection<BsonDocument>("AspNetUsers");
        var legacyUser = new BsonDocument
        {
            { "_id", ObjectId.GenerateNewId() },
            { "UserName", "NoConcurrencyUser" },
            { "Email", "noconcurrency@example.com" },
            { "Roles", new BsonArray() },
            { "Claims", new BsonArray() },
            { "Logins", new BsonArray() }
        };
        await usersCollection.InsertOneAsync(legacyUser);

        // Reset migration marker to force re-migration
        await MigrationHelper.ResetMigrationAsync(_database);

        // Act - Create UserStore which triggers migration
        using var userStore = new UserStore<IdentityUser>(_database);
        var foundUser = await userStore.FindByNameAsync("NOCONCURRENCYUSER");

        // Assert
        foundUser.Should().NotBeNull();
        foundUser!.ConcurrencyStamp.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Migration_ShouldCreateIndexes()
    {
        // Arrange
        await MigrationHelper.ResetMigrationAsync(_database);

        // Act - Create UserStore which triggers migration
        using var userStore = new UserStore<IdentityUser>(_database);
        // Trigger migration by performing a database operation
        await userStore.CreateAsync(new IdentityUser("IndexTestUser") { Email = "indextest@example.com" });

        // Assert - Check that indexes were created
        var usersCollection = _database.GetCollection<BsonDocument>("AspNetUsers");
        var indexes = await (await usersCollection.Indexes.ListAsync()).ToListAsync();

        var indexNames = indexes.Select(i => i["name"].AsString).ToList();
        indexNames.Should().Contain("IX_NormalizedUserName");
        indexNames.Should().Contain("IX_NormalizedEmail");
        indexNames.Should().Contain("IX_Logins");
        indexNames.Should().Contain("IX_Roles");
        indexNames.Should().Contain("IX_Claims");
    }

    [Fact]
    public async Task Migration_ShouldOnlyRunOnce()
    {
        // Arrange
        await MigrationHelper.ResetMigrationAsync(_database);

        // Act - Create multiple UserStores
        using var userStore1 = new UserStore<IdentityUser>(_database);
        await userStore1.CreateAsync(new IdentityUser("User1") { Email = "user1@example.com" });

        using var userStore2 = new UserStore<IdentityUser>(_database);
        await userStore2.CreateAsync(new IdentityUser("User2") { Email = "user2@example.com" });

        // Assert - Check migration collection only has one entry
        var migrationCollection = _database.GetCollection<BsonDocument>("_IdentityMigrations");
        var migrationCount = await migrationCollection.CountDocumentsAsync(Builders<BsonDocument>.Filter.Empty);
        migrationCount.Should().Be(1);
    }

    [Fact]
    public async Task Migration_ShouldMigrateRoles()
    {
        // Arrange - Insert a legacy role document without NormalizedName
        var rolesCollection = _database.GetCollection<BsonDocument>("AspNetRoles");
        var legacyRole = new BsonDocument
        {
            { "_id", "role1" },
            { "Name", "Admin" }
        };
        await rolesCollection.InsertOneAsync(legacyRole);

        // Reset migration marker to force re-migration
        await MigrationHelper.ResetMigrationAsync(_database);

        // Act - Create RoleStore which triggers migration
        using var roleStore = new RoleStore(_database);
        var foundRole = await roleStore.FindByNameAsync("ADMIN");

        // Assert
        foundRole.Should().NotBeNull();
        foundRole!.Name.Should().Be("Admin");
        foundRole.NormalizedName.Should().Be("ADMIN");
    }
}
