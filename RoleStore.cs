using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDB.AspNet.Identity
{
    /// <summary>
    /// Represents a new instance of a persistence store for roles, using MongoDB.
    /// </summary>
    public class RoleStore : IRoleStore<IdentityRole>, IQueryableRoleStore<IdentityRole>
    {
        private readonly IMongoDatabase _database;
        private bool _disposed;
        private bool _migrationEnsured;
        private readonly SemaphoreSlim _migrationLock = new(1, 1);

        private const string CollectionName = "AspNetRoles";

        private IMongoCollection<IdentityRole> RolesCollection => _database.GetCollection<IdentityRole>(CollectionName);

        /// <summary>
        /// Gets an <see cref="IQueryable{IdentityRole}"/> of roles.
        /// </summary>
        public IQueryable<IdentityRole> Roles => RolesCollection.AsQueryable();

        /// <summary>
        /// Initializes a new instance of <see cref="RoleStore"/> using the specified MongoDB database.
        /// </summary>
        /// <param name="database">The MongoDB database to use.</param>
        public RoleStore(IMongoDatabase database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        #region Migration

        private async Task EnsureMigrationAsync(CancellationToken cancellationToken)
        {
            if (_migrationEnsured) return;

            await _migrationLock.WaitAsync(cancellationToken);
            try
            {
                if (_migrationEnsured) return;

                await MigrationHelper.EnsureRolesMigratedAsync(_database, CollectionName, cancellationToken);
                _migrationEnsured = true;
            }
            finally
            {
                _migrationLock.Release();
            }
        }

        #endregion

        /// <inheritdoc/>
        public async Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(role);

            await EnsureMigrationAsync(cancellationToken);
            await RolesCollection.InsertOneAsync(role, cancellationToken: cancellationToken);
            return IdentityResult.Success;
        }

        /// <inheritdoc/>
        public async Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(role);

            await RolesCollection.DeleteOneAsync(
                Builders<IdentityRole>.Filter.Eq(r => r.Id, role.Id),
                cancellationToken);
            return IdentityResult.Success;
        }

        /// <inheritdoc/>
        public async Task<IdentityRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            await EnsureMigrationAsync(cancellationToken);
            return await RolesCollection
                .Find(Builders<IdentityRole>.Filter.Eq(r => r.Id, roleId))
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IdentityRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            await EnsureMigrationAsync(cancellationToken);
            return await RolesCollection
                .Find(Builders<IdentityRole>.Filter.Eq(r => r.NormalizedName, normalizedRoleName))
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public Task<string?> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(role);

            return Task.FromResult(role.NormalizedName);
        }

        /// <inheritdoc/>
        public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(role);

            return Task.FromResult(role.Id);
        }

        /// <inheritdoc/>
        public Task<string?> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(role);

            return Task.FromResult(role.Name);
        }

        /// <inheritdoc/>
        public Task SetNormalizedRoleNameAsync(IdentityRole role, string? normalizedName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(role);

            role.NormalizedName = normalizedName;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SetRoleNameAsync(IdentityRole role, string? roleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(role);

            role.Name = roleName;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(role);

            await RolesCollection.ReplaceOneAsync(
                Builders<IdentityRole>.Filter.Eq(r => r.Id, role.Id),
                role,
                new ReplaceOptions { IsUpsert = false },
                cancellationToken);

            return IdentityResult.Success;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        protected void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }
    }
}
