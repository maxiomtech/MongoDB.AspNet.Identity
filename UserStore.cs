// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserStore.cs" company="">
//
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace MongoDB.AspNet.Identity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using Microsoft.AspNet.Identity;

    using MongoDB.Bson;
    using MongoDB.Driver;

    /// <summary>
    /// Class UserStore.
    /// </summary>
    /// <typeparam name="TUser">
    /// The type of the user.
    /// </typeparam>
    public class UserStore<TUser> : UserStore<TUser, IdentityRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>,
                                    IUserStore<TUser>
        where TUser : IdentityUser
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserStore{TUser}"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public UserStore(IdentityDbContext context)
            : base(context.Database)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserStore{TUser}"/> class.
        /// </summary>
        /// <param name="database">
        /// The database.
        /// </param>
        public UserStore(IMongoDatabase database)
            : base(database)
        {
        }

        #endregion
    }

    /// <summary>
    /// Class UserStore.
    /// </summary>
    /// <typeparam name="TUser">
    /// The type of the user.
    /// </typeparam>
    /// <typeparam name="TRole">
    /// The type of the role.
    /// </typeparam>
    /// <typeparam name="TKey">
    /// The type of the key.
    /// </typeparam>
    /// <typeparam name="TUserLogin">
    /// The type of the user login.
    /// </typeparam>
    /// <typeparam name="TUserRole">
    /// The type of the user role.
    /// </typeparam>
    /// <typeparam name="TUserClaim">
    /// The type of the user claim.
    /// </typeparam>
    public class UserStore<TUser, TRole, TKey, TUserLogin, TUserRole, TUserClaim> : IUserLoginStore<TUser, TKey>,
                                                                                    IUserClaimStore<TUser, TKey>,
                                                                                    IUserRoleStore<TUser, TKey>,
                                                                                    IUserPasswordStore<TUser, TKey>,
                                                                                    IUserSecurityStampStore<TUser, TKey>,
                                                                                    IQueryableUserStore<TUser, TKey>,
                                                                                    IUserEmailStore<TUser, TKey>,
                                                                                    IUserPhoneNumberStore<TUser, TKey>,
                                                                                    IUserTwoFactorStore<TUser, TKey>
        where TUser : IdentityUser<TKey, TUserLogin, TUserRole, TUserClaim>
        where TRole : IdentityRole<TKey, TUserRole>
        where TKey : IEquatable<TKey>
        where TUserLogin : IdentityUserLogin<TKey>, new()
        where TUserRole : IdentityUserRole<TKey>, new()
        where TUserClaim : IdentityUserClaim<TKey>, new()
    {
        #region Fields

        /// <summary>
        ///     The AspNetUsers collection name
        /// </summary>
        private readonly string collectionName = "AspNetUsers";

        /// <summary>
        ///     The database
        /// </summary>
        private readonly IMongoDatabase db;

        /// <summary>
        ///     The _disposed
        /// </summary>
        private bool disposed;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the users.
        /// </summary>
        /// <value>
        ///     The users.
        /// </value>
        public IQueryable<TUser> Users { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Adds the <paramref name="claim"/> asynchronous.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="claim">
        /// The claim.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="user"/>
        /// </exception>
        /// <returns>
        /// A Task.
        /// </returns>
        public Task AddClaimAsync([NotNull]TUser user, [NotNull]Claim claim)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (!user.Claims.Any(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value))
            {
                user.Claims.Add(new IdentityUserClaim { ClaimType = claim.Type, ClaimValue = claim.Value });
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Adds the <paramref name="login"/> asynchronously.
        /// </summary>
        /// <param name="user">
        /// The <paramref name="user"/> .
        /// </param>
        /// <param name="login">
        /// The <paramref name="user"/> login information.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// The <paramref name="user"/> cannot be <see langword="null"/> .
        /// </exception>
        /// <returns>
        /// A Task.
        /// </returns>
        public Task AddLoginAsync([NotNull]TUser user, [NotNull]UserLoginInfo login)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (!user.Logins.Any(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey))
            {
                user.Logins.Add(login);
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// Adds to <paramref name="role"/> asynchronous.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="role">
        /// The role.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="user"/>
        /// </exception>
        /// <returns>
        /// A Task.
        /// </returns>
        public Task AddToRoleAsync([NotNull]TUser user, [NotNull]string role)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (!user.Roles.Contains(role, StringComparer.InvariantCultureIgnoreCase))
            {
                user.Roles.Add(role);
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// Creates the <paramref name="user"/> asynchronous.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="user"/>
        /// </exception>
        /// <returns>
        /// A Task.
        /// </returns>
        public async Task CreateAsync([NotNull]TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            await this.db.GetCollection<TUser>(this.collectionName).InsertOneAsync(user).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes the <paramref name="user"/> asynchronous.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="user"/>
        /// </exception>
        /// <returns>
        /// A Task.
        /// </returns>
        public async Task DeleteAsync([NotNull]TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var filter = new FilterDefinitionBuilder<TUser>().Eq("_id", ObjectId.Parse(user.Id.ToString()));
            await this.db.GetCollection<TUser>(this.collectionName).DeleteOneAsync(filter).ConfigureAwait(false);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.disposed = true;
        }

        /// <summary>
        /// Finds the user asynchronous.
        /// </summary>
        /// <param name="login">
        /// The <paramref name="login"/>.
        /// </param>
        /// <returns>
        /// A Task{TUser}.
        /// </returns>
        public async Task<TUser> FindAsync([NotNull]UserLoginInfo login)
        {
            var filter = Builders<TUser>.Filter.And(
                Builders<TUser>.Filter.Eq("Logins.LoginProvider", login.LoginProvider),
                Builders<TUser>.Filter.Eq("Logins.ProviderKey", login.ProviderKey));

            var cursor = await this.db.GetCollection<TUser>(this.collectionName).FindAsync<TUser>(filter).ConfigureAwait(false);
            var users = await cursor.ToListAsync().ConfigureAwait(false);
            var user = users.FirstOrDefault();

            return user;
        }

        /// <summary>
        /// Finds the by <paramref name="email"/> asynchronous.
        /// </summary>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <returns>
        /// A Task{TUser}.
        /// </returns>
        public async Task<TUser> FindByEmailAsync([NotNull]string email)
        {
            this.ThrowIfDisposed();

            var cursor = await this.db.GetCollection<TUser>(this.collectionName).FindAsync<TUser>(Builders<TUser>.Filter.Eq("Email", email)).ConfigureAwait(false);
            var users = await cursor.ToListAsync().ConfigureAwait(false);
            var user = users.FirstOrDefault();

            return user;
        }

        /// <summary>
        /// Finds the by identifier asynchronous.
        /// </summary>
        /// <param name="userId">
        /// The user identifier.
        /// </param>
        /// <returns>
        /// A Task{<typeparamref name="TUser"/>}.
        /// </returns>
        public async Task<TUser> FindByIdAsync([NotNull]TKey userId)
        {
            this.ThrowIfDisposed();

            var cursor = await this.db.GetCollection<TUser>(this.collectionName).FindAsync<TUser>(Builders<TUser>.Filter.Eq("_id", ObjectId.Parse(userId.ToString()))).ConfigureAwait(false);
            var users = await cursor.ToListAsync().ConfigureAwait(false);
            var user = users.FirstOrDefault();

            return user;
        }

        /// <summary>
        /// Finds the by name asynchronous.
        /// </summary>
        /// <param name="userName">
        /// Name of the user.
        /// </param>
        /// <returns>
        /// A Task{TUser}.
        /// </returns>
        public async Task<TUser> FindByNameAsync([NotNull]string userName)
        {
            this.ThrowIfDisposed();

            var cursor = await this.db.GetCollection<TUser>(this.collectionName).FindAsync<TUser>(Builders<TUser>.Filter.Eq("UserName", ObjectId.Parse(userName))).ConfigureAwait(false);
            var users = await cursor.ToListAsync().ConfigureAwait(false);
            var user = users.FirstOrDefault();

            return user;
        }

        /// <summary>
        /// Gets the claims asynchronous.
        /// </summary>
        /// <param name="user">
        /// The <paramref name="user"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="user"/>
        /// </exception>
        /// <returns>
        /// A Task{IList{Claim}}.
        /// </returns>
        public Task<IList<Claim>> GetClaimsAsync([NotNull]TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            IList<Claim> result = user.Claims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList();
            return Task.FromResult(result);
        }

        /// <summary>
        /// Gets the email asynchronous.
        /// </summary>
        /// <param name="user">
        /// The <paramref name="user"/>.
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// <paramref name="user"/>
        /// </exception>
        /// <returns>
        /// A Task{System.String}.
        /// </returns>
        public Task<string> GetEmailAsync([NotNull]TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentException(nameof(user));
            }

            return Task.FromResult(user.Email);
        }

        /// <summary>
        /// Gets the email confirmed asynchronous.
        /// </summary>
        /// <param name="user">
        /// The <paramref name="user"/>.
        /// </param>
        /// <exception cref="System.NotImplementedException">
        /// Not implemented.
        /// </exception>
        /// <returns>
        /// A Task{System.Boolean}.
        /// </returns>
        public Task<bool> GetEmailConfirmedAsync([NotNull]TUser user)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the logins asynchronous.
        /// </summary>
        /// <param name="user">
        /// The <paramref name="user"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="user"/>
        /// </exception>
        /// <returns>
        /// A Task{IList{UserLoginInfo}}.
        /// </returns>
        public Task<IList<UserLoginInfo>> GetLoginsAsync([NotNull]TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.Logins.ToIList());
        }

        /// <summary>
        /// Gets the password hash asynchronous.
        /// </summary>
        /// <param name="user">
        /// The <paramref name="user"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="user"/>
        /// </exception>
        /// <returns>
        /// A Task{System.String}.
        /// </returns>
        public Task<string> GetPasswordHashAsync([NotNull]TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.PasswordHash);
        }

        /// <summary>
        /// Gets the phone number asynchronous.
        /// </summary>
        /// <param name="user">
        /// The <paramref name="user"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="user"/>
        /// </exception>
        /// <returns>
        /// A Task{System.String}.
        /// </returns>
        public Task<string> GetPhoneNumberAsync([NotNull]TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.PhoneNumber);
        }

        /// <summary>
        /// Gets the phone number confirmed asynchronous.
        /// </summary>
        /// <param name="user">
        /// The <paramref name="user"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="user"/>
        /// </exception>
        /// <returns>
        /// A Task{System.Boolean}.
        /// </returns>
        public Task<bool> GetPhoneNumberConfirmedAsync([NotNull]TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        /// <summary>
        /// Gets the roles asynchronous.
        /// </summary>
        /// <param name="user">
        /// The <paramref name="user"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="user"/>
        /// </exception>
        /// <returns>
        /// A Task{IList{System.String}}.
        /// </returns>
        public Task<IList<string>> GetRolesAsync([NotNull]TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult<IList<string>>(user.Roles);
        }

        /// <summary>
        /// Gets the security stamp asynchronous.
        /// </summary>
        /// <param name="user">
        /// The <paramref name="user"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="user"/>
        /// </exception>
        /// <returns>
        /// A Task{System.String}.
        /// </returns>
        public Task<string> GetSecurityStampAsync([NotNull]TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.SecurityStamp);
        }

        /// <summary>
        /// Gets the two factor enabled asynchronous.
        /// </summary>
        /// <param name="user">
        /// The <paramref name="user"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="user"/>
        /// </exception>
        /// <returns>
        /// A Task{System.Boolean}.
        /// </returns>
        public Task<bool> GetTwoFactorEnabledAsync([NotNull]TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.TwoFactorEnabled);
        }

        /// <summary>
        /// Determines whether [has password asynchronous] [the specified <paramref name="user"/>].
        /// </summary>
        /// <param name="user">
        /// The <paramref name="user"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="user"/>
        /// </exception>
        /// <returns>
        /// The <see cref="System.Threading.Tasks.Task"/> .
        /// </returns>
        public Task<bool> HasPasswordAsync([NotNull]TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.PasswordHash != null);
        }

        /// <summary>
        /// Determines whether [is in <paramref name="role"/> asynchronous] [the
        ///     specified user].
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="role">
        /// The role.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="user"/>
        /// </exception>
        /// <returns>
        /// The <see cref="System.Threading.Tasks.Task"/> .
        /// </returns>
        public Task<bool> IsInRoleAsync([NotNull]TUser user, [NotNull]string role)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.Roles.Contains(role, StringComparer.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Removes the <paramref name="claim"/> asynchronous.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="claim">
        /// The claim.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="user"/>
        /// </exception>
        /// <returns>
        /// A Task.
        /// </returns>
        public Task RemoveClaimAsync([NotNull]TUser user, [NotNull]Claim claim)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.Claims.RemoveAll(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);
            return Task.FromResult(0);
        }

        /// <summary>
        /// Removes from <paramref name="role"/> asynchronous.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="role">
        /// The role.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="user"/>
        /// </exception>
        /// <returns>
        /// A Task.
        /// </returns>
        public Task RemoveFromRoleAsync([NotNull]TUser user, [NotNull] string role)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.Roles.RemoveAll(r => string.Equals(r, role, StringComparison.InvariantCultureIgnoreCase));

            return Task.FromResult(0);
        }

        /// <summary>
        /// Removes the <paramref name="login"/> asynchronous.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="login">
        /// The login.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="user"/>
        /// </exception>
        /// <returns>
        /// A Task.
        /// </returns>
        public Task RemoveLoginAsync([NotNull]TUser user, [NotNull] UserLoginInfo login)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.Logins.RemoveAll(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey);

            return Task.FromResult(0);
        }

        /// <summary>
        /// Sets the <paramref name="email"/> asynchronous.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <exception cref="System.NotImplementedException">
        /// Not implemented.
        /// </exception>
        /// <returns>
        /// A Task.
        /// </returns>
        public Task SetEmailAsync([NotNull]TUser user, [NotNull] string email)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the email <paramref name="confirmed"/> asynchronous.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="confirmed">
        /// if set to <see langword="true"/> [confirmed].
        /// </param>
        /// <exception cref="System.NotImplementedException">
        /// Not implemented.
        /// </exception>
        /// <returns>
        /// A Task.
        /// </returns>
        public Task SetEmailConfirmedAsync([NotNull]TUser user, bool confirmed)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the password hash asynchronous.
        /// </summary>
        /// <param name="user">
        /// The <paramref name="user"/>.
        /// </param>
        /// <param name="passwordHash">
        /// The password hash.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="user"/>
        /// </exception>
        /// <returns>
        /// A Task.
        /// </returns>
        public Task SetPasswordHashAsync([NotNull]TUser user, [NotNull]string passwordHash)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        /// <summary>
        /// Sets the phone number asynchronous.
        /// </summary>
        /// <param name="user">
        /// The <paramref name="user"/>.
        /// </param>
        /// <param name="phoneNumber">
        /// The phone number.
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// <paramref name="user"/>
        /// </exception>
        /// <returns>
        /// A Task.
        /// </returns>
        public Task SetPhoneNumberAsync([NotNull]TUser user, [NotNull]string phoneNumber)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentException(nameof(user));
            }

            user.PhoneNumber = phoneNumber;
            return Task.FromResult(0);
        }

        /// <summary>
        /// Sets the phone number <paramref name="confirmed"/> asynchronous.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="confirmed">
        /// if set to <c>true</c> [confirmed].
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="user"/>
        /// </exception>
        /// <returns>
        /// A Task.
        /// </returns>
        public Task SetPhoneNumberConfirmedAsync([NotNull]TUser user, bool confirmed)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.PhoneNumberConfirmed = confirmed;
            return Task.FromResult(0);
        }

        /// <summary>
        /// Sets the security <paramref name="stamp"/> asynchronous.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="stamp">
        /// The stamp.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="user"/>
        /// </exception>
        /// <returns>
        /// A Task.
        /// </returns>
        public Task SetSecurityStampAsync([NotNull]TUser user, [NotNull]string stamp)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        /// <summary>
        /// Sets the two factor <paramref name="enabled"/> asynchronous.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="enabled">
        /// if set to <see langword="true"/> [enabled].
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="user"/>
        /// </exception>
        /// <returns>
        /// A Task.
        /// </returns>
        public Task SetTwoFactorEnabledAsync([NotNull]TUser user, bool enabled)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.TwoFactorEnabled = enabled;
            return Task.FromResult(0);
        }

        /// <summary>
        /// Updates the <paramref name="user"/> asynchronous.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="user"/>
        /// </exception>
        /// <returns>
        /// A Task.
        /// </returns>
        public async Task UpdateAsync([NotNull]TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            await
                this.db.GetCollection<TUser>(this.collectionName)
                    .ReplaceOneAsync(Builders<TUser>.Filter.Eq("_id", ObjectId.Parse(user.Id.ToString())), user, new UpdateOptions { IsUpsert = true })
                    .ConfigureAwait(false);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Throws if <see cref="disposed"/>.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        /// Attempt to use a disposed object.
        /// </exception>
        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserStore{TUser,TRole,TKey,TUserLogin,TUserRole,TUserClaim}"/> class.
        /// </summary>
        public UserStore()
            : this(new IdentityDbContext().Database)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserStore{TUser,TRole,TKey,TUserLogin,TUserRole,TUserClaim}"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public UserStore(IMongoDatabase context)
        {
            this.db = context;
        }

        #endregion
    }
}
