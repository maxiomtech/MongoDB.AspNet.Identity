using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDB.AspNet.Identity
{
    /// <summary>
    /// Represents a new instance of a persistence store for users, using MongoDB.
    /// </summary>
    /// <typeparam name="TUser">The type representing a user.</typeparam>
    public class UserStore<TUser> :
        IUserStore<TUser>,
        IUserLoginStore<TUser>,
        IUserClaimStore<TUser>,
        IUserRoleStore<TUser>,
        IUserPasswordStore<TUser>,
        IUserSecurityStampStore<TUser>,
        IUserEmailStore<TUser>,
        IUserLockoutStore<TUser>,
        IUserTwoFactorStore<TUser>,
        IUserPhoneNumberStore<TUser>,
        IUserAuthenticationTokenStore<TUser>,
        IQueryableUserStore<TUser>
        where TUser : IdentityUser
    {
        private readonly IMongoDatabase _database;
        private bool _disposed;

        /// <summary>
        /// The default collection name for users.
        /// </summary>
        private const string CollectionName = "AspNetUsers";

        /// <summary>
        /// Gets the users collection.
        /// </summary>
        private IMongoCollection<TUser> UsersCollection => _database.GetCollection<TUser>(CollectionName);

        /// <summary>
        /// Gets an <see cref="IQueryable{TUser}"/> of users.
        /// </summary>
        public IQueryable<TUser> Users => UsersCollection.AsQueryable();

        /// <summary>
        /// Initializes a new instance of <see cref="UserStore{TUser}"/> using the specified MongoDB connection string.
        /// </summary>
        /// <param name="connectionString">The MongoDB connection string (must include database name).</param>
        public UserStore(string connectionString)
        {
            var mongoUrl = new MongoUrl(connectionString);
            if (string.IsNullOrEmpty(mongoUrl.DatabaseName))
            {
                throw new ArgumentException("The connection string must include a database name.", nameof(connectionString));
            }

            var client = new MongoClient(mongoUrl);
            _database = client.GetDatabase(mongoUrl.DatabaseName);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UserStore{TUser}"/> using the specified MongoDB connection string and database name.
        /// </summary>
        /// <param name="connectionString">The MongoDB connection string.</param>
        /// <param name="databaseName">The name of the database.</param>
        public UserStore(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UserStore{TUser}"/> using an existing MongoDB database.
        /// </summary>
        /// <param name="database">The MongoDB database to use.</param>
        public UserStore(IMongoDatabase database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        #region IUserStore

        /// <inheritdoc/>
        public async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            await UsersCollection.InsertOneAsync(user, cancellationToken: cancellationToken);
            return IdentityResult.Success;
        }

        /// <inheritdoc/>
        public async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            await UsersCollection.DeleteOneAsync(
                Builders<TUser>.Filter.Eq("_id", ObjectId.Parse(user.Id)),
                cancellationToken);
            return IdentityResult.Success;
        }

        /// <inheritdoc/>
        public async Task<TUser?> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (!ObjectId.TryParse(userId, out var objectId))
            {
                return null;
            }

            return await UsersCollection
                .Find(Builders<TUser>.Filter.Eq("_id", objectId))
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<TUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return await UsersCollection
                .Find(Builders<TUser>.Filter.Eq(u => u.NormalizedUserName, normalizedUserName))
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public Task<string?> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            return Task.FromResult(user.NormalizedUserName);
        }

        /// <inheritdoc/>
        public Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            return Task.FromResult(user.Id);
        }

        /// <inheritdoc/>
        public Task<string?> GetUserNameAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            return Task.FromResult(user.UserName);
        }

        /// <inheritdoc/>
        public Task SetNormalizedUserNameAsync(TUser user, string? normalizedName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SetUserNameAsync(TUser user, string? userName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            user.UserName = userName;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            user.ConcurrencyStamp = Guid.NewGuid().ToString();

            await UsersCollection.ReplaceOneAsync(
                Builders<TUser>.Filter.Eq("_id", ObjectId.Parse(user.Id)),
                user,
                new ReplaceOptions { IsUpsert = false },
                cancellationToken);

            return IdentityResult.Success;
        }

        #endregion

        #region IUserLoginStore

        /// <inheritdoc/>
        public Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(login);

            if (!user.Logins.Any(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey))
            {
                user.Logins.Add(new IdentityUserLogin
                {
                    LoginProvider = login.LoginProvider,
                    ProviderKey = login.ProviderKey,
                    ProviderDisplayName = login.ProviderDisplayName
                });
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<TUser?> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var filter = Builders<TUser>.Filter.And(
                Builders<TUser>.Filter.Eq("Logins.LoginProvider", loginProvider),
                Builders<TUser>.Filter.Eq("Logins.ProviderKey", providerKey));

            return await UsersCollection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            IList<UserLoginInfo> logins = user.Logins
                .Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, l.ProviderDisplayName))
                .ToList();

            return Task.FromResult(logins);
        }

        /// <inheritdoc/>
        public Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            user.Logins.RemoveAll(x => x.LoginProvider == loginProvider && x.ProviderKey == providerKey);
            return Task.CompletedTask;
        }

        #endregion

        #region IUserClaimStore

        /// <inheritdoc/>
        public Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(claims);

            foreach (var claim in claims)
            {
                if (!user.Claims.Any(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value))
                {
                    var identityClaim = new IdentityUserClaim();
                    identityClaim.InitializeFromClaim(claim);
                    user.Claims.Add(identityClaim);
                }
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            IList<Claim> claims = user.Claims.Select(c => c.ToClaim()).ToList();
            return Task.FromResult(claims);
        }

        /// <inheritdoc/>
        public async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(claim);

            var filter = Builders<TUser>.Filter.And(
                Builders<TUser>.Filter.Eq("Claims.ClaimType", claim.Type),
                Builders<TUser>.Filter.Eq("Claims.ClaimValue", claim.Value));

            return await UsersCollection.Find(filter).ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(claims);

            foreach (var claim in claims)
            {
                user.Claims.RemoveAll(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(claim);
            ArgumentNullException.ThrowIfNull(newClaim);

            var existingClaim = user.Claims.FirstOrDefault(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);
            if (existingClaim != null)
            {
                existingClaim.InitializeFromClaim(newClaim);
            }

            return Task.CompletedTask;
        }

        #endregion

        #region IUserRoleStore

        /// <inheritdoc/>
        public Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);
            ArgumentException.ThrowIfNullOrWhiteSpace(roleName);

            if (!user.Roles.Contains(roleName, StringComparer.OrdinalIgnoreCase))
            {
                user.Roles.Add(roleName);
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            return Task.FromResult<IList<string>>(user.Roles.ToList());
        }

        /// <inheritdoc/>
        public async Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentException.ThrowIfNullOrWhiteSpace(roleName);

            var filter = Builders<TUser>.Filter.AnyEq(u => u.Roles, roleName);
            return await UsersCollection.Find(filter).ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);
            ArgumentException.ThrowIfNullOrWhiteSpace(roleName);

            return Task.FromResult(user.Roles.Contains(roleName, StringComparer.OrdinalIgnoreCase));
        }

        /// <inheritdoc/>
        public Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);
            ArgumentException.ThrowIfNullOrWhiteSpace(roleName);

            user.Roles.RemoveAll(r => string.Equals(r, roleName, StringComparison.OrdinalIgnoreCase));
            return Task.CompletedTask;
        }

        #endregion

        #region IUserPasswordStore

        /// <inheritdoc/>
        public Task<string?> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            return Task.FromResult(user.PasswordHash);
        }

        /// <inheritdoc/>
        public Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
        }

        /// <inheritdoc/>
        public Task SetPasswordHashAsync(TUser user, string? passwordHash, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        #endregion

        #region IUserSecurityStampStore

        /// <inheritdoc/>
        public Task<string?> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            return Task.FromResult(user.SecurityStamp);
        }

        /// <inheritdoc/>
        public Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            user.SecurityStamp = stamp;
            return Task.CompletedTask;
        }

        #endregion

        #region IUserEmailStore

        /// <inheritdoc/>
        public async Task<TUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return await UsersCollection
                .Find(Builders<TUser>.Filter.Eq(u => u.NormalizedEmail, normalizedEmail))
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public Task<string?> GetEmailAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            return Task.FromResult(user.Email);
        }

        /// <inheritdoc/>
        public Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            return Task.FromResult(user.EmailConfirmed);
        }

        /// <inheritdoc/>
        public Task<string?> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            return Task.FromResult(user.NormalizedEmail);
        }

        /// <inheritdoc/>
        public Task SetEmailAsync(TUser user, string? email, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            user.Email = email;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SetNormalizedEmailAsync(TUser user, string? normalizedEmail, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            user.NormalizedEmail = normalizedEmail;
            return Task.CompletedTask;
        }

        #endregion

        #region IUserLockoutStore

        /// <inheritdoc/>
        public Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            return Task.FromResult(user.AccessFailedCount);
        }

        /// <inheritdoc/>
        public Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            return Task.FromResult(user.LockoutEnabled);
        }

        /// <inheritdoc/>
        public Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            return Task.FromResult(user.LockoutEnd);
        }

        /// <inheritdoc/>
        public Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            user.AccessFailedCount++;
            return Task.FromResult(user.AccessFailedCount);
        }

        /// <inheritdoc/>
        public Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            user.AccessFailedCount = 0;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            user.LockoutEnabled = enabled;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            user.LockoutEnd = lockoutEnd;
            return Task.CompletedTask;
        }

        #endregion

        #region IUserTwoFactorStore

        /// <inheritdoc/>
        public Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            return Task.FromResult(user.TwoFactorEnabled);
        }

        /// <inheritdoc/>
        public Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            user.TwoFactorEnabled = enabled;
            return Task.CompletedTask;
        }

        #endregion

        #region IUserPhoneNumberStore

        /// <inheritdoc/>
        public Task<string?> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            return Task.FromResult(user.PhoneNumber);
        }

        /// <inheritdoc/>
        public Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        /// <inheritdoc/>
        public Task SetPhoneNumberAsync(TUser user, string? phoneNumber, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            user.PhoneNumber = phoneNumber;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            user.PhoneNumberConfirmed = confirmed;
            return Task.CompletedTask;
        }

        #endregion

        #region IUserAuthenticationTokenStore

        /// <inheritdoc/>
        public Task<string?> GetTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            var token = user.Tokens.FirstOrDefault(t => t.LoginProvider == loginProvider && t.Name == name);
            return Task.FromResult(token?.Value);
        }

        /// <inheritdoc/>
        public Task RemoveTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            user.Tokens.RemoveAll(t => t.LoginProvider == loginProvider && t.Name == name);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SetTokenAsync(TUser user, string loginProvider, string name, string? value, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(user);

            var existingToken = user.Tokens.FirstOrDefault(t => t.LoginProvider == loginProvider && t.Name == name);
            if (existingToken != null)
            {
                existingToken.Value = value;
            }
            else
            {
                user.Tokens.Add(new IdentityUserToken
                {
                    LoginProvider = loginProvider,
                    Name = name,
                    Value = value
                });
            }

            return Task.CompletedTask;
        }

        #endregion

        #region IDisposable

        /// <inheritdoc/>
        public void Dispose()
        {
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Throws if this class has been disposed.
        /// </summary>
        protected void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }

        #endregion
    }
}
