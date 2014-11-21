using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;

namespace MongoDB.AspNet.Identity
{

    /// <summary>
    /// Class UserStore.
    /// </summary>
    /// <typeparam name="TUser">The type of the t user.</typeparam>
    public class UserStore<TUser> : UserStore<TUser, IdentityRole, string, IdentityUserLogin, IdentityUserRole, IdentityDbContext>, IDisposable
    where TUser : IdentityUser


    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserStore{TUser}"/> class.
        /// <param name="context">The context.</param>
        /// </summary>
        public UserStore(IdentityDbContext context) : base(context)
        {

        }
    }

    /// <summary>
    /// Class UserStore.
    /// </summary>
    /// <typeparam name="TUser">The type of the t user.</typeparam>
    /// <typeparam name="TRole">The type of the t role.</typeparam>
    /// <typeparam name="TKey">The type of the t key.</typeparam>
    /// <typeparam name="TUserLogin">The type of the t user login.</typeparam>
    /// <typeparam name="TUserRole">The type of the t user role.</typeparam>
    /// <typeparam name="TContext">context to access database</typeparam>

    public class UserStore<TUser, TRole, TKey, TUserLogin, TUserRole, TContext> :

        IUserLoginStore<TUser>,
        IUserClaimStore<TUser>,
        IUserRoleStore<TUser>,
        IUserPasswordStore<TUser>,
        IUserSecurityStampStore<TUser>,
        IUserEmailStore<TUser>,
        IUserLockoutStore<TUser>,
        IUserPhoneNumberStore<TUser>,
        IQueryableUserStore<TUser>,
        IUserTwoFactorStore<TUser>
        where TKey : IEquatable<TKey>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TContext : IdentityDbContext
        where TUserLogin : IdentityUserLogin<TKey>, new()
        where TUserRole : IdentityUserRole<TKey>, new()
    {
        #region Private Methods & Variables


        /// <summary>
        ///     The database
        /// </summary>
        private readonly MongoDatabase db;

        /// <summary>
        ///     The _disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// The AspNetUsers collection name
        /// </summary>
        private const string collectionName = "AspNetUsers";

        /// <summary>
        /// Gets the users.
        /// </summary>
        /// <value>The users.</value>
        public IQueryable<TUser> Users
        {
            get
            {
                return db.GetCollection<TUser>(collectionName).AsQueryable();
            }
        }


        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserStore{TUser, TRole, TKey, TUserLogin, TUserRole}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public UserStore(TContext context)
        {
            db = context.Database;
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
        public Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            foreach (var claim in claims)
            {
                if (!user.Claims.Any(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value))
                {
                    user.Claims.Add(new IdentityUserClaim
                    {
                        ClaimType = claim.Type,
                        ClaimValue = claim.Value
                    });
                }
            }
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Gets the claims asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task{IList{Claim}}.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            IList<Claim> result = user.Claims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList();
            return Task.FromResult(result);
        }

        public Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }
            if (newClaim == null)
            {
                throw new ArgumentNullException("newClaim");
            }

            var matchedClaims = user.Claims.Where(uc => uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type).ToList();
            foreach (var matchedClaim in matchedClaims)
            {
                matchedClaim.ClaimValue = newClaim.Value;
                matchedClaim.ClaimType = newClaim.Type;
            }

            return Task.FromResult(0);
        }


        /// <summary>
        ///     Removes the claim asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="claim">The claim.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            foreach (var claim in claims)
            {
                var matchedClaims = user.Claims.Where(uc => uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type).ToList();
                foreach (var c in matchedClaims)
                {
                    user.Claims.RemoveAll(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);
                    //user.Claims.Remove(c);
                }
            }
            return Task.FromResult(0);
        }


        /// <summary>
        ///     Creates the user asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task CreateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            db.GetCollection<TUser>(collectionName).Insert(user);

            return Task.FromResult(user);
        }

        /// <summary>
        ///     Deletes the user asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task DeleteAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            db.GetCollection(collectionName).Remove((Query.EQ("_id", ObjectId.Parse(user.Id.ToString()))));
            return Task.FromResult(true);
        }

        /// <summary>
        /// Finds the by identifier asynchronous.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>Task{`0}.</returns>
        public Task<TUser> FindByIdAsync(TKey userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            //TUser user = db.GetCollection<TUser>(collectionName).FindOne((Query.EQ("_id", ObjectId.Parse(userId.ToString()))));
            TUser user = Users.FirstOrDefault(i => i.Id.Equals(userId));
            return Task.FromResult(user);
        }

        /// <summary>
        ///     Finds the by identifier asynchronous.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>Task{`0}.</returns>
        public Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            //TUser user = db.GetCollection<TUser>(collectionName).FindOne((Query.EQ("_id", ObjectId.Parse(userId))));
            TUser user = Users.FirstOrDefault(i => i.Id.Equals(userId));
            return Task.FromResult(user);
        }

        /// <summary>
        ///     Finds the by name asynchronous.
        /// </summary>
        /// <param name="NormalizedUserName">Name of the user.</param>
        /// <returns>Task{`0}.</returns>
        public Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();

            //TUser user = db.GetCollection<TUser>(collectionName).FindOne((Query.EQ("NormalizedUserName", normalizedUserName)));
            TUser user = Users.AsQueryable().FirstOrDefault(n => n.NormalizedUserName.Equals(normalizedUserName));
            return Task.FromResult(user);
        }

        /// <summary>
        /// Sets the email confirmed asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="confirmed">if set to <c>true</c> [confirmed].</param>
        /// <returns>Task.</returns>

        public Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();

            user.EmailConfirmed = true;
            return Task.FromResult(0);
        }

        /// <summary>
        /// Finds the by email asynchronous.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns>Task{`0}.</returns>
        public Task<TUser> FindByEmailAsync(string email, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();

            //TUser user = db.GetCollection<TUser>(collectionName).FindOne((Query.EQ("Email", email)));
            TUser user = Users.AsQueryable().FirstOrDefault(x => x.Email.Equals(email));
            return Task.FromResult(user);
        }

        /// <summary>
        /// Sets the email asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="email">The email.</param>
        /// <returns>Task.</returns>        
        public Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentException("user");
            }
            user.Email = email;
            return Task.FromResult(0);
        }

        /// <summary>
        /// Gets the email asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task{System.String}.</returns>
        /// <exception cref="System.ArgumentException">user</exception>
        public Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentException("user");
            }
            return Task.FromResult<string>(user.Email);
        }

        /// <summary>
        /// Gets the email confirmed asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task{System.Boolean}.</returns>

        public Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentException("user");
            }
            return Task.FromResult(user.EmailConfirmed);
        }

        /// <summary>
        ///     Updates the user asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task UpdateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            db.GetCollection<TUser>(collectionName).Update(Query.EQ("_id", ObjectId.Parse(user.Id.ToString())), Update.Replace(user), UpdateFlags.Upsert);
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
        public Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken = default(CancellationToken))
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
        ///     Gets the logins asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task{IList{UserLoginInfo}}.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
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
        //public Task RemoveLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    ThrowIfDisposed();
        //    if (user == null)
        //        throw new ArgumentNullException("user");

        //    user.Logins.RemoveAll(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey);

        //    return Task.FromResult(0);
        //}
        public Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.Logins.RemoveAll(x => x.LoginProvider == loginProvider && x.ProviderKey == providerKey);

            return Task.FromResult(0);
        }
        /// <summary>
        ///     Gets the password hash asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task{System.String}.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
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
        public Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
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
        public Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken = default(CancellationToken))
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
        public Task AddToRoleAsync(TUser user, string role, CancellationToken cancellationToken = default(CancellationToken))
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
        public Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
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
        public Task<bool> IsInRoleAsync(TUser user, string role, CancellationToken cancellationToken = default(CancellationToken))
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
        public Task RemoveFromRoleAsync(TUser user, string role, CancellationToken cancellationToken = default(CancellationToken))
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
        public Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
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
        public Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        /// <summary>
        /// Sets the phone number asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="phoneNumber">The phone number.</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentException">user</exception>
        public Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentException("user");
            }

            user.PhoneNumber = phoneNumber;
            return Task.FromResult<int>(0);
        }

        /// <summary>
        /// Gets the phone number asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task{System.String}.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return Task.FromResult<string>(user.PhoneNumber);
        }

        /// <summary>
        /// Gets the phone number confirmed asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task{System.Boolean}.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<bool>(user.PhoneNumberConfirmed);
        }

        /// <summary>
        /// Sets the phone number confirmed asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="confirmed">if set to <c>true</c> [confirmed].</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.PhoneNumberConfirmed = confirmed;
            return Task.FromResult<int>(0);
        }

        /// <summary>
        /// Sets the two factor enabled asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        /// <returns>Task.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.TwoFactorEnabled = enabled;
            return Task.FromResult<int>(0);
        }

        /// <summary>
        /// Gets the two factor enabled asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Task{System.Boolean}.</returns>
        /// <exception cref="System.ArgumentNullException">user</exception>
        public Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<bool>(user.TwoFactorEnabled);
        }

        public Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            // todo: ensure logins loaded

            //var user = db.GetCollection<TUser>(collectionName).FindOne((Query.And(Query.EQ("Logins.LoginProvider", loginProvider), Query.EQ("Logins.ProviderKey", providerKey))));
            var user = Users.AsQueryable().FirstOrDefault(x => x.Logins.Any(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey));

            return Task.FromResult<TUser>(user);
        }

        public Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.Id != null ? user.Id.ToString() : null);
        }

        public Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.UserName);
        }

        public Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.UserName = userName;
            return Task.FromResult(0);
        }

        public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task SetNormalizedUserNameAsync(TUser user, string userName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.NormalizedUserName = userName;
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Returns the DateTimeOffset that represents the end of a user's lockout, any time in the past should be considered
        ///     not locked out.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task<DateTimeOffset> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.LockoutEnd);
        }

        /// <summary>
        ///     Locks a user out until the specified end date (set to a past date, to unlock a user)
        /// </summary>
        /// <param name="user"></param>
        /// <param name="lockoutEnd"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task SetLockoutEndDateAsync(TUser user, DateTimeOffset lockoutEnd, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.LockoutEnd = lockoutEnd;
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Used to record when an attempt to access the user has failed
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.AccessFailedCount++;
            return Task.FromResult(user.AccessFailedCount);
        }

        /// <summary>
        ///     Used to reset the account access count, typically after the account is successfully accessed
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.AccessFailedCount = 0;
            return Task.FromResult(0);
        }

        /// <summary>
        ///     Returns the current number of failed access attempts.  This number usually will be reset whenever the password is
        ///     verified or the account is locked out.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.AccessFailedCount);
        }

        /// <summary>
        ///     Returns whether the user can be locked out.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.LockoutEnabled);
        }

        /// <summary>
        ///     Sets whether the user can be locked out.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="enabled"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.LockoutEnabled = enabled;
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


    }
}
