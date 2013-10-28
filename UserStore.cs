using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.AspNet.Identity;

namespace MongoDB.AspNet.Identity
{
    public class UserStore<TUser> : IUserStore<TUser>, IUserLoginStore<TUser>, IUserClaimStore<TUser>, IUserRoleStore<TUser>,
        IUserPasswordStore<TUser>, IUserSecurityStampStore<TUser>
        where TUser : IdentityUser
    {
        private bool _disposed;

        private MongoDatabase dbContext;

        public UserStore(string databaseName)
        {
            var conString =
                    new MongoConnectionStringBuilder(ConfigurationManager.ConnectionStrings[databaseName].ConnectionString);
            MongoClientSettings settings = MongoClientSettings.FromConnectionStringBuilder(conString);
            MongoServer server = new MongoClient(settings).GetServer();
            dbContext = server.GetDatabase(conString.DatabaseName);
        }

        public Task CreateAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            dbContext.GetCollection<TUser>("AspNetUsers").Insert(user);

            return Task.FromResult(true);
        }

        //private string UsernameToDocumentId(string userName)
        //{
        //    var conventions = session.Advanced.DocumentStore.Conventions;
        //    string typeTagName = conventions.GetTypeTagName(typeof (TUser));
        //    string tag = conventions.TransformTypeTagNameToDocumentKeyPrefix(typeTagName);
        //    return String.Format("{0}{1}{2}", tag, conventions.IdentityPartsSeparator, userName);
        //}

        public Task DeleteAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            dbContext.GetCollection("AspNetUsers").Remove((Query.EQ("_id", ObjectId.Parse(user.Id))));
            return Task.FromResult(true);
        }

        public Task<TUser> FindByIdAsync(string userId)
        {
            //var user = this.session.Load<TUser>(userId);
            var user = dbContext.GetCollection<TUser>("AspNetUsers").FindOne((Query.EQ("_id", ObjectId.Parse(userId))));
            return Task.FromResult(user);
        }

        public Task<TUser> FindByNameAsync(string userName)
        {
            var user = dbContext.GetCollection<TUser>("AspNetUsers").FindOne((Query.EQ("UserName", userName)));
            return Task.FromResult(user);
        }

        public Task UpdateAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            dbContext.GetCollection<TUser>("AspNetUsers").Update(Query.EQ("_id", ObjectId.Parse(user.Id)), Update.Replace(user), UpdateFlags.Upsert);

            return Task.FromResult(true);
        }

        private void ThrowIfDisposed()
        {
            if (this._disposed)
                throw new ObjectDisposedException(this.GetType().Name);
        }

        public void Dispose()
        {
            this._disposed = true;
        }

        public Task AddLoginAsync(TUser user, UserLoginInfo login)
        {
            this.ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            if (!user.Logins.Any(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey))
            {
                user.Logins.Add(login);

                dbContext.GetCollection("AspNetUserLogins").Insert(new IdentityUserLogin
                {
                    UserId = user.Id,
                    LoginProvider = login.LoginProvider,
                    ProviderKey = login.ProviderKey
                });

            }

            return Task.FromResult(true);
        }

        public Task<TUser> FindAsync(UserLoginInfo login)
        {
            TUser user = null;
            var userLogin =
                dbContext.GetCollection<IdentityUserLogin>("AspNetUserLogins")
                    .FindOne(Query.And(Query.EQ("LoginProvider", login.LoginProvider),
                        Query.EQ("ProviderKey", login.ProviderKey)));
            if (userLogin != null)
                user = dbContext.GetCollection<TUser>("AspNetUsers").FindOneById(ObjectId.Parse(userLogin.UserId));

            return Task.FromResult(user);
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.Logins.ToIList());
        }

        public Task RemoveLoginAsync(TUser user, UserLoginInfo login)
        {
            this.ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            var userLogin =
                dbContext.GetCollection<IdentityUserLogin>("AspNetUserLogins")
                    .Remove(Query.And(Query.EQ("LoginProvider", login.LoginProvider),
                        Query.EQ("ProviderKey", login.ProviderKey)));

            user.Logins.RemoveAll(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey);

            return Task.FromResult(0);
        }

        public Task AddClaimAsync(TUser user, Claim claim)
        {
            this.ThrowIfDisposed();
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

        public Task<IList<Claim>> GetClaimsAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            IList<Claim> result = user.Claims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList();
            return Task.FromResult(result);
        }

        public Task RemoveClaimAsync(TUser user, Claim claim)
        {
            this.ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.Claims.RemoveAll(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);
            return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult<bool>(user.PasswordHash != null);
        }

        public Task SetPasswordHashAsync(TUser user, string passwordHash)
        {
            this.ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public Task<string> GetSecurityStampAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.SecurityStamp);
        }

        public Task SetSecurityStampAsync(TUser user, string stamp)
        {
            this.ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        public Task AddToRoleAsync(TUser user, string role)
        {
            this.ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            if (!user.Roles.Contains(role, StringComparer.InvariantCultureIgnoreCase))
                user.Roles.Add(role);

            return Task.FromResult(0);
        }

        public Task<IList<string>> GetRolesAsync(TUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult<IList<string>>(user.Roles);
        }

        public Task<bool> IsInRoleAsync(TUser user, string role)
        {
            this.ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            return Task.FromResult(user.Roles.Contains(role, StringComparer.InvariantCultureIgnoreCase));
        }

        public Task RemoveFromRoleAsync(TUser user, string role)
        {
            this.ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException("user");

            user.Roles.RemoveAll(r => String.Equals(r, role, StringComparison.InvariantCultureIgnoreCase));

            return Task.FromResult(0);
        }
    }
}
