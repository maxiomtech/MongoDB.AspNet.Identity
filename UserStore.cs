using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDB.AspNet.Identity
{
    /// <summary>
    ///     Class UserStore.
    /// </summary>
    /// <typeparam name="TUser">The type of the t user.</typeparam>
    public class UserStore<TUser> :
        IUserStore<TUser>,
        IUserLoginStore<TUser>, 
        IUserClaimStore<TUser>, 
        IUserRoleStore<TUser>,
        IUserPasswordStore<TUser>, 
        IUserSecurityStampStore<TUser>,
        IUserEmailStore<TUser>,
        IUserLockoutStore<TUser, string>,
        IUserTwoFactorStore<TUser, string>
        where TUser : IdentityUser
    {
        #region Private Methods & Variables

        /// <summary>
        ///     The database
        /// </summary>
        private readonly IMongoDatabase db;

        /// <summary>
        ///     The _disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// The AspNetUsers collection name
        /// </summary>
        private const string collectionName = "AspNetUsers";

        /// <summary>
        ///     Gets the database from connection string.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>MongoDatabase.</returns>
        /// <exception cref="System.Exception">No database name specified in connection string</exception>
        private IMongoDatabase GetDatabaseFromSqlStyle(string connectionString)
        {
            var mongoUrl = new MongoUrl(connectionString);
            MongoClientSettings settings = MongoClientSettings.FromUrl(mongoUrl);
            IMongoClient client = new MongoClient(settings);
            if (mongoUrl.DatabaseName == null)
            {
                throw new Exception("No database name specified in connection string");
            }
            return client.GetDatabase(mongoUrl.DatabaseName);
        }

        /// <summary>
        ///     Gets the database from URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>MongoDatabase.</returns>
        private IMongoDatabase GetDatabaseFromUrl(MongoUrl url)
        {
            IMongoClient client = new MongoClient(url);
            if (url.DatabaseName == null)
            {
                throw new Exception("No database name specified in connection string");
            }
            return client.GetDatabase(url.DatabaseName); // WriteConcern defaulted to Acknowledged
        }

        /// <summary>
        ///     Uses connectionString to connect to server and then uses databae name specified.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="dbName">Name of the database.</param>
        /// <returns>MongoDatabase.</returns>
        private IMongoDatabase GetDatabase(string connectionString, string dbName)
        {
            var client = new MongoClient(connectionString);
            return client.GetDatabase(dbName);
        }

        #endregion

        #region Constructors
        
        /// <summary>
        ///     Initializes a new instance of the <see cref="UserStore{TUser}" /> class. Uses DefaultConnection name if none was
        ///     specified.
        /// </summary>
        public UserStore() 
            : this("DefaultConnection")
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserStore{TUser}" /> class. Uses name from ConfigurationManager or a
        ///     mongodb:// Url.
        /// </summary>
        /// <param name="connectionNameOrUrl">The connection name or URL.</param>
        public UserStore(string connectionNameOrUrl)
        {
            if (connectionNameOrUrl.ToLower().StartsWith("mongodb://"))
            {
                db = GetDatabaseFromUrl(new MongoUrl(connectionNameOrUrl));
            }
            else
            {
                string connStringFromManager =
                    ConfigurationManager.ConnectionStrings[connectionNameOrUrl].ConnectionString;
                db = connStringFromManager.ToLower().StartsWith("mongodb://") 
                    ? GetDatabaseFromUrl(new MongoUrl(connStringFromManager)) 
                    : GetDatabaseFromSqlStyle(connStringFromManager);
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserStore{TUser}" /> class. Uses name from ConfigurationManager or a
        ///     mongodb:// Url.
        ///     Database can be specified separately from connection server.
        /// </summary>
        /// <param name="connectionNameOrUrl">The connection name or URL.</param>
        /// <param name="dbName">Name of the database.</param>
        public UserStore(string connectionNameOrUrl, string dbName)
        {
            if (connectionNameOrUrl.ToLower().StartsWith("mongodb://"))
            {
                db = GetDatabase(connectionNameOrUrl, dbName);
            }
            else
            {
                db = GetDatabase(ConfigurationManager.ConnectionStrings[connectionNameOrUrl].ConnectionString, dbName);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserStore{TUser}"/> class using a already initialized Mongo Database.
        /// </summary>
        /// <param name="mongoDatabase">The mongo database.</param>
        public UserStore(IMongoDatabase mongoDatabase)
        {
            db = mongoDatabase;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Adds the claim asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="claim">The claim.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task AddClaimAsync(TUser user, Claim claim)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            if (!user.Claims.Any(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value))
            {
                user.Claims.Add(new IdentityUserClaim
                {
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value
                });
            }
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Gets the claims asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task{IList{Claim}}.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task<IList<Claim>> GetClaimsAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            IList<Claim> result = user.Claims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList();
            return Task.FromResult(result);
        }

        /// <summary>
        ///     Removes the claim asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="claim">The claim.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task RemoveClaimAsync(TUser user, Claim claim)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.Claims.RemoveAll(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);
            return Task.FromResult(0);
        }


        /// <summary>
        ///     Creates the user asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task CreateAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            db.GetCollection<TUser>(collectionName).InsertOneAsync(user);
            return Task.FromResult(user);
        }

        /// <summary>
        ///     Deletes the user asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task DeleteAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            db.GetCollection<TUser>(collectionName)
                .DeleteOneAsync(Builders<TUser>.Filter.Eq("_id", ObjectId.Parse(user.Id)));
            return Task.FromResult(true);
        }

        /// <summary>
        ///     Finds the by identifier asynchronous.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>Task{`0}.</returns>
        public Task<TUser> FindByIdAsync(string userId)
        {
            ThrowIfDisposed();
            var user = db.GetCollection<TUser>(collectionName)
                .Find(Builders<TUser>.Filter.Eq("_id", ObjectId.Parse(userId)))
                .FirstOrDefaultAsync();
            return user;
        }

        /// <summary>
        ///     Finds the by name asynchronous.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>Task{`0}.</returns>
        public Task<TUser> FindByNameAsync(string userName)
        {
            ThrowIfDisposed();
            var user = db.GetCollection<TUser>(collectionName)
                .Find(Builders<TUser>.Filter.Eq("UserName", userName))
                .FirstOrDefaultAsync();
            return user;
        }

        /// <summary>
        ///     Updates the user asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task UpdateAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            db.GetCollection<TUser>(collectionName)
                .FindOneAndReplaceAsync(
                Builders<TUser>.Filter.Eq("_id", ObjectId.Parse(user.Id)), 
                user, 
                new FindOneAndReplaceOptions<TUser> {IsUpsert = true});

            return Task.FromResult(user);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _disposed = true;
        }

        /// <summary>
        ///     Adds the login asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="login">The login.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task AddLoginAsync(TUser user, UserLoginInfo login)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            if (!user.Logins.Any(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey))
            {
                user.Logins.Add(login);
            }
            return Task.FromResult(true);
        }

        /// <summary>
        ///     Finds the user asynchronous.
        /// </summary>
        /// <param name="login">The login.</param>
        /// <returns>Task{`0}.</returns>
        public Task<TUser> FindAsync(UserLoginInfo login)
        {
            var filter = Builders<TUser>.Filter;
            var user = db.GetCollection<TUser>(collectionName)
                .Find(filter.And(filter.Eq("Logins.LoginProvider", login.LoginProvider),
                    filter.Eq("Logins.ProviderKey", login.ProviderKey))).FirstOrDefaultAsync();
            return user;
        }

        /// <summary>
        ///     Gets the logins asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task{IList{UserLoginInfo}}.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            return Task.FromResult(user.Logins.ToIList());
        }

        /// <summary>
        ///     Removes the login asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="login">The login.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task RemoveLoginAsync(TUser user, UserLoginInfo login)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            user.Logins.RemoveAll(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey);
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Gets the password hash asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task{System.String}.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task<string> GetPasswordHashAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            return Task.FromResult(user.PasswordHash);
        }

        /// <summary>
        ///     Determines whether [has password asynchronous] [the specified user].
        /// </summary>
        /// <param name="user">The user.</param>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task<bool> HasPasswordAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            return Task.FromResult(user.PasswordHash != null);
        }

        /// <summary>
        ///     Sets the password hash asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="passwordHash">The password hash.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task SetPasswordHashAsync(TUser user, string passwordHash)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Adds to role asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="role">The role.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task AddToRoleAsync(TUser user, string role)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            if (!user.Roles.Contains(role, StringComparer.InvariantCultureIgnoreCase))
                user.Roles.Add(role);

            return Task.FromResult(true);
        }

        /// <summary>
        ///     Gets the roles asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task{IList{System.String}}.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task<IList<string>> GetRolesAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            return Task.FromResult<IList<string>>(user.Roles);
        }

        /// <summary>
        ///     Determines whether [is in role asynchronous] [the specified user].
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="role">The role.</param>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task<bool> IsInRoleAsync(TUser user, string role)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.Roles.Contains(role, StringComparer.InvariantCultureIgnoreCase));
        }

        /// <summary>
        ///     Removes from role asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="role">The role.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task RemoveFromRoleAsync(TUser user, string role)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.Roles.RemoveAll(r => String.Equals(r, role, StringComparison.InvariantCultureIgnoreCase));

            return Task.FromResult(0);
        }

        /// <summary>
        ///     Gets the security stamp asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task{System.String}.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task<string> GetSecurityStampAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.SecurityStamp);
        }

        /// <summary>
        ///     Sets the security stamp asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="stamp">The stamp.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task SetSecurityStampAsync(TUser user, string stamp)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Throws if disposed.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException"></exception>
        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        #endregion

        /// <summary>Set the user email</summary>
        /// <param name="user"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public Task SetEmailAsync(TUser user, string email)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.Email = email;
            return Task.FromResult(0);
        }

        /// <summary>Get the user email</summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<string> GetEmailAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.Email);
        }

        /// <summary>Returns true if the user email is confirmed</summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<bool> GetEmailConfirmedAsync(TUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            return Task.FromResult(user.EmailConfirmed);
        }

        /// <summary>Sets whether the user email is confirmed</summary>
        /// <param name="user"></param>
        /// <param name="confirmed"></param>
        /// <returns></returns>
        public Task SetEmailConfirmedAsync(TUser user, bool confirmed)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        /// <summary>Returns the user associated with this email</summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public Task<TUser> FindByEmailAsync(string email)
        {
            ThrowIfDisposed();
            var user = db.GetCollection<TUser>(collectionName)
                .Find(Builders<TUser>.Filter.Eq("Email", email))
                .FirstOrDefaultAsync();
            return user;
        }

        /// <summary>
        ///     Returns the DateTimeOffset that represents the end of a user's lockout, any time in the past should be considered
        ///     not locked out.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<DateTimeOffset> GetLockoutEndDateAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return await Task.FromResult(user.LockoutEndDateUtc.HasValue
                    ? new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc))
                    : new DateTimeOffset());
        }

        /// <summary>
        ///     Locks a user out until the specified end date (set to a past date, to unlock a user)
        /// </summary>
        /// <param name="user"></param>
        /// <param name="lockoutEnd"></param>
        /// <returns></returns>
        public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset lockoutEnd)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.LockoutEndDateUtc = lockoutEnd.UtcDateTime;
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Used to record when an attempt to access the user has failed
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<int> IncrementAccessFailedCountAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.AccessFailedCount++;
            return Task.FromResult(user.AccessFailedCount);
        }

        /// <summary>
        ///     Used to reset the access failed count, typically after the account is successfully accessed
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task ResetAccessFailedCountAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.AccessFailedCount = 0;
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Returns the current number of failed access attempts.  This number usually will be reset whenever the password is
        ///     verified or the account is locked out.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<int> GetAccessFailedCountAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.AccessFailedCount);
        }

        /// <summary>Returns whether the user can be locked out.</summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<bool> GetLockoutEnabledAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.LockoutEnabled);
        }

        /// <summary>Sets whether the user can be locked out.</summary>
        /// <param name="user"></param>
        /// <param name="enabled"></param>
        /// <returns></returns>
        public Task SetLockoutEnabledAsync(TUser user, bool enabled)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.LockoutEnabled = enabled;
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Sets whether two factor authentication is enabled for the user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="enabled"></param>
        /// <returns></returns>
        public Task SetTwoFactorEnabledAsync(TUser user, bool enabled)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.TwoFactorEnabled = enabled;
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Returns whether two factor authentication is enabled for the user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<bool> GetTwoFactorEnabledAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.TwoFactorEnabled);
        }
    }
}