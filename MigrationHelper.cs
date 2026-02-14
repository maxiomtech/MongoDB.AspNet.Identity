using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDB.AspNet.Identity
{
    /// <summary>
    /// Handles automatic migration of legacy ASP.NET Identity documents to ASP.NET Core Identity format
    /// and ensures proper indexes exist.
    /// </summary>
    public static class MigrationHelper
    {
        private const string MigrationCollectionName = "_IdentityMigrations";
        private const string UsersMigrationKey = "AspNetUsers_v2";
        private const string RolesMigrationKey = "AspNetRoles_v2";

        /// <summary>
        /// Ensures the users collection is migrated and indexed.
        /// This method is idempotent and safe to call multiple times.
        /// </summary>
        /// <typeparam name="TUser">The user type.</typeparam>
        /// <param name="database">The MongoDB database.</param>
        /// <param name="collectionName">The users collection name.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public static async Task EnsureUsersMigratedAsync<TUser>(
            IMongoDatabase database,
            string collectionName = "AspNetUsers",
            CancellationToken cancellationToken = default)
            where TUser : IdentityUser
        {
            if (await IsMigrationCompletedAsync(database, UsersMigrationKey, cancellationToken))
            {
                return;
            }

            var collection = database.GetCollection<BsonDocument>(collectionName);

            // Migrate documents that don't have NormalizedUserName
            await MigrateUsersAsync(collection, cancellationToken);

            // Create indexes
            await CreateUserIndexesAsync(collection, cancellationToken);

            // Mark migration as complete
            await MarkMigrationCompletedAsync(database, UsersMigrationKey, cancellationToken);
        }

        /// <summary>
        /// Ensures the roles collection is migrated and indexed.
        /// This method is idempotent and safe to call multiple times.
        /// </summary>
        /// <param name="database">The MongoDB database.</param>
        /// <param name="collectionName">The roles collection name.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public static async Task EnsureRolesMigratedAsync(
            IMongoDatabase database,
            string collectionName = "AspNetRoles",
            CancellationToken cancellationToken = default)
        {
            if (await IsMigrationCompletedAsync(database, RolesMigrationKey, cancellationToken))
            {
                return;
            }

            var collection = database.GetCollection<BsonDocument>(collectionName);

            // Migrate documents that don't have NormalizedName
            await MigrateRolesAsync(collection, cancellationToken);

            // Create indexes
            await CreateRoleIndexesAsync(collection, cancellationToken);

            // Mark migration as complete
            await MarkMigrationCompletedAsync(database, RolesMigrationKey, cancellationToken);
        }

        private static async Task<bool> IsMigrationCompletedAsync(
            IMongoDatabase database,
            string migrationKey,
            CancellationToken cancellationToken)
        {
            var migrationCollection = database.GetCollection<BsonDocument>(MigrationCollectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", migrationKey);
            var doc = await migrationCollection.Find(filter).FirstOrDefaultAsync(cancellationToken);
            return doc != null;
        }

        private static async Task MarkMigrationCompletedAsync(
            IMongoDatabase database,
            string migrationKey,
            CancellationToken cancellationToken)
        {
            var migrationCollection = database.GetCollection<BsonDocument>(MigrationCollectionName);
            var doc = new BsonDocument
            {
                { "_id", migrationKey },
                { "completedAt", DateTime.UtcNow },
                { "version", "2.0.0" }
            };

            await migrationCollection.ReplaceOneAsync(
                Builders<BsonDocument>.Filter.Eq("_id", migrationKey),
                doc,
                new ReplaceOptions { IsUpsert = true },
                cancellationToken);
        }

        private static async Task MigrateUsersAsync(
            IMongoCollection<BsonDocument> collection,
            CancellationToken cancellationToken)
        {
            // Find all documents that need migration (NormalizedUserName doesn't exist or is null)
            var filter = Builders<BsonDocument>.Filter.Or(
                Builders<BsonDocument>.Filter.Exists("NormalizedUserName", false),
                Builders<BsonDocument>.Filter.Eq("NormalizedUserName", BsonNull.Value)
            );

            var cursor = await collection.FindAsync(filter, cancellationToken: cancellationToken);
            var batch = new List<WriteModel<BsonDocument>>();

            await cursor.ForEachAsync(doc =>
            {
                var updates = new List<UpdateDefinition<BsonDocument>>();

                // Normalize UserName
                if (doc.Contains("UserName") && doc["UserName"] != BsonNull.Value)
                {
                    var userName = doc["UserName"].AsString;
                    updates.Add(Builders<BsonDocument>.Update.Set("NormalizedUserName", userName.ToUpperInvariant()));
                }

                // Normalize Email
                if (doc.Contains("Email") && doc["Email"] != BsonNull.Value)
                {
                    var email = doc["Email"].AsString;
                    updates.Add(Builders<BsonDocument>.Update.Set("NormalizedEmail", email.ToUpperInvariant()));
                }

                // Convert LockoutEndDateUtc to LockoutEnd if it exists
                if (doc.Contains("LockoutEndDateUtc") && doc["LockoutEndDateUtc"] != BsonNull.Value)
                {
                    var lockoutEndUtc = doc["LockoutEndDateUtc"].ToUniversalTime();
                    updates.Add(Builders<BsonDocument>.Update.Set("LockoutEnd", lockoutEndUtc));
                    updates.Add(Builders<BsonDocument>.Update.Unset("LockoutEndDateUtc"));
                }

                // Add ConcurrencyStamp if missing
                if (!doc.Contains("ConcurrencyStamp") || doc["ConcurrencyStamp"] == BsonNull.Value)
                {
                    updates.Add(Builders<BsonDocument>.Update.Set("ConcurrencyStamp", Guid.NewGuid().ToString()));
                }

                // Add PhoneNumberConfirmed if missing
                if (!doc.Contains("PhoneNumberConfirmed"))
                {
                    updates.Add(Builders<BsonDocument>.Update.Set("PhoneNumberConfirmed", false));
                }

                // Initialize Tokens array if missing
                if (!doc.Contains("Tokens"))
                {
                    updates.Add(Builders<BsonDocument>.Update.Set("Tokens", new BsonArray()));
                }

                // Migrate Logins from UserLoginInfo format to IdentityUserLogin format
                if (doc.Contains("Logins") && doc["Logins"].IsBsonArray)
                {
                    var logins = doc["Logins"].AsBsonArray;
                    var migratedLogins = new BsonArray();
                    var needsLoginMigration = false;

                    foreach (var login in logins)
                    {
                        if (login.IsBsonDocument)
                        {
                            var loginDoc = login.AsBsonDocument;
                            // Check if it's old format (has ProviderKey but no ProviderDisplayName might be missing)
                            if (!loginDoc.Contains("ProviderDisplayName"))
                            {
                                needsLoginMigration = true;
                                loginDoc["ProviderDisplayName"] = loginDoc.Contains("LoginProvider")
                                    ? loginDoc["LoginProvider"]
                                    : BsonNull.Value;
                            }
                            migratedLogins.Add(loginDoc);
                        }
                    }

                    if (needsLoginMigration)
                    {
                        updates.Add(Builders<BsonDocument>.Update.Set("Logins", migratedLogins));
                    }
                }

                if (updates.Count > 0)
                {
                    var combinedUpdate = Builders<BsonDocument>.Update.Combine(updates);
                    batch.Add(new UpdateOneModel<BsonDocument>(
                        Builders<BsonDocument>.Filter.Eq("_id", doc["_id"]),
                        combinedUpdate));
                }
            }, cancellationToken);

            if (batch.Count > 0)
            {
                await collection.BulkWriteAsync(batch, cancellationToken: cancellationToken);
            }
        }

        private static async Task MigrateRolesAsync(
            IMongoCollection<BsonDocument> collection,
            CancellationToken cancellationToken)
        {
            // Find all documents that need migration
            var filter = Builders<BsonDocument>.Filter.Or(
                Builders<BsonDocument>.Filter.Exists("NormalizedName", false),
                Builders<BsonDocument>.Filter.Eq("NormalizedName", BsonNull.Value)
            );

            var cursor = await collection.FindAsync(filter, cancellationToken: cancellationToken);
            var batch = new List<WriteModel<BsonDocument>>();

            await cursor.ForEachAsync(doc =>
            {
                var updates = new List<UpdateDefinition<BsonDocument>>();

                // Normalize Name
                if (doc.Contains("Name") && doc["Name"] != BsonNull.Value)
                {
                    var name = doc["Name"].AsString;
                    updates.Add(Builders<BsonDocument>.Update.Set("NormalizedName", name.ToUpperInvariant()));
                }

                // Add ConcurrencyStamp if missing
                if (!doc.Contains("ConcurrencyStamp") || doc["ConcurrencyStamp"] == BsonNull.Value)
                {
                    updates.Add(Builders<BsonDocument>.Update.Set("ConcurrencyStamp", Guid.NewGuid().ToString()));
                }

                if (updates.Count > 0)
                {
                    var combinedUpdate = Builders<BsonDocument>.Update.Combine(updates);
                    batch.Add(new UpdateOneModel<BsonDocument>(
                        Builders<BsonDocument>.Filter.Eq("_id", doc["_id"]),
                        combinedUpdate));
                }
            }, cancellationToken);

            if (batch.Count > 0)
            {
                await collection.BulkWriteAsync(batch, cancellationToken: cancellationToken);
            }
        }

        private static async Task CreateUserIndexesAsync(
            IMongoCollection<BsonDocument> collection,
            CancellationToken cancellationToken)
        {
            var indexes = new List<CreateIndexModel<BsonDocument>>
            {
                // Unique index on NormalizedUserName for fast username lookups
                // Using partial filter to only index non-null values (allows multiple users without NormalizedUserName during migration)
                new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Ascending("NormalizedUserName"),
                    new CreateIndexOptions<BsonDocument>
                    {
                        Unique = true,
                        Name = "IX_NormalizedUserName",
                        PartialFilterExpression = Builders<BsonDocument>.Filter.And(
                            Builders<BsonDocument>.Filter.Exists("NormalizedUserName"),
                            Builders<BsonDocument>.Filter.Type("NormalizedUserName", BsonType.String))
                    }),

                // Unique index on NormalizedEmail for email lookups
                new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Ascending("NormalizedEmail"),
                    new CreateIndexOptions<BsonDocument>
                    {
                        Unique = true,
                        Name = "IX_NormalizedEmail",
                        PartialFilterExpression = Builders<BsonDocument>.Filter.And(
                            Builders<BsonDocument>.Filter.Exists("NormalizedEmail"),
                            Builders<BsonDocument>.Filter.Type("NormalizedEmail", BsonType.String))
                    }),

                // Compound index on Logins for external login lookups
                new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys
                        .Ascending("Logins.LoginProvider")
                        .Ascending("Logins.ProviderKey"),
                    new CreateIndexOptions { Sparse = true, Name = "IX_Logins" }),

                // Index on Roles for GetUsersInRole queries
                new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Ascending("Roles"),
                    new CreateIndexOptions { Sparse = true, Name = "IX_Roles" }),

                // Index on Claims for GetUsersForClaim queries
                new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys
                        .Ascending("Claims.ClaimType")
                        .Ascending("Claims.ClaimValue"),
                    new CreateIndexOptions { Sparse = true, Name = "IX_Claims" })
            };

            try
            {
                await collection.Indexes.CreateManyAsync(indexes, cancellationToken);
            }
            catch (MongoCommandException ex) when (ex.Code == 85 || ex.Code == 86)
            {
                // Index already exists with different options - try to create individually
                foreach (var index in indexes)
                {
                    try
                    {
                        await collection.Indexes.CreateOneAsync(index, cancellationToken: cancellationToken);
                    }
                    catch (MongoCommandException)
                    {
                        // Index exists, skip
                    }
                }
            }
        }

        private static async Task CreateRoleIndexesAsync(
            IMongoCollection<BsonDocument> collection,
            CancellationToken cancellationToken)
        {
            var indexes = new List<CreateIndexModel<BsonDocument>>
            {
                // Unique index on NormalizedName for fast role lookups
                // Using partial filter to only index non-null values
                new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Ascending("NormalizedName"),
                    new CreateIndexOptions<BsonDocument>
                    {
                        Unique = true,
                        Name = "IX_NormalizedName",
                        PartialFilterExpression = Builders<BsonDocument>.Filter.And(
                            Builders<BsonDocument>.Filter.Exists("NormalizedName"),
                            Builders<BsonDocument>.Filter.Type("NormalizedName", BsonType.String))
                    })
            };

            try
            {
                await collection.Indexes.CreateManyAsync(indexes, cancellationToken);
            }
            catch (MongoCommandException ex) when (ex.Code == 85 || ex.Code == 86)
            {
                // Index already exists with different options
                foreach (var index in indexes)
                {
                    try
                    {
                        await collection.Indexes.CreateOneAsync(index, cancellationToken: cancellationToken);
                    }
                    catch (MongoCommandException)
                    {
                        // Index exists, skip
                    }
                }
            }
        }

        /// <summary>
        /// Forces re-running the migration by clearing the migration markers.
        /// Use this if you need to re-migrate after schema changes.
        /// </summary>
        /// <param name="database">The MongoDB database.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public static async Task ResetMigrationAsync(
            IMongoDatabase database,
            CancellationToken cancellationToken = default)
        {
            var migrationCollection = database.GetCollection<BsonDocument>(MigrationCollectionName);
            await migrationCollection.DeleteManyAsync(
                Builders<BsonDocument>.Filter.Empty,
                cancellationToken);
        }
    }
}
