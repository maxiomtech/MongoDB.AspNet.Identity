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

namespace MongoDB.AspNet.Identity
{
    public class RoleStore<TRole> : RoleStore<TRole, string>      
    where TRole : IdentityRole, new()
    {
        public RoleStore(IdentityDbContext context) : base(context) { }
    }   

    public class RoleStore<TRole, TKey> : 
        IQueryableRoleStore<TRole>, 
        IRoleClaimStore<TRole>, 
        IDisposable
        where TKey : IEquatable<TKey>
        where TRole : IdentityRole<TKey>
        //where TClaim : IdentityUserClaim<TKey>
        //where TUserRole : IdentityUserRole<TKey>, new()
    {
        private bool _disposed;
        
        private readonly MongoDatabase db;
        private const string collectionName = "AspNetRoles";

        public RoleStore(IdentityDbContext context)
        {
            db = context.Database;
        }

        public bool DisposeContext
        {
            get;
            set;
        }

        public IQueryable<TRole> Roles
        {
            get { return db.GetCollection<TRole>(collectionName).FindAll().AsQueryable(); }
        }

        public IQueryable<IdentityRoleClaim> RoleClaims
        {
            get { return db.GetCollection<IdentityRoleClaim>(collectionName).FindAll().AsQueryable(); }
        }
		
		public virtual Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            this.ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }

            db.GetCollection<TRole>(collectionName).Insert(role);
            return Task.FromResult(IdentityResult.Success);
        }

        public virtual Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            this.ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }

            db.GetCollection(collectionName).Remove((Query.EQ("_id", ObjectId.Parse(role.Id.ToString()))));

            return Task.FromResult(IdentityResult.Success);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _disposed = true;
        }

        public Task<TRole> FindByIdAsync(TKey roleId, CancellationToken cancellationToken = default(CancellationToken))
        {
            this.ThrowIfDisposed();
            //TRole role = db.GetCollection<TRole>(collectionName).FindOne((Query.EQ("_id", ObjectId.Parse(roleId.ToString()))));
            TRole role = Roles.FirstOrDefault(i => i.Id.Equals(roleId));
            return Task.FromResult(role);
        }

        public Task<TRole> FindByNameAsync(string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            this.ThrowIfDisposed();
            //TRole role = db.GetCollection<TRole>(collectionName).FindOne((Query.EQ("Name", roleName)));
            TRole role = Roles.FirstOrDefault(i => i.Name.Equals(roleName));
            return Task.FromResult(role);
        }

        private void ThrowIfDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        public virtual Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            this.ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }

            db.GetCollection<TRole>(collectionName).Update(Query.EQ("_id", ObjectId.Parse(role.Id.ToString())), Update.Replace(role), UpdateFlags.Upsert);

            return Task.FromResult(IdentityResult.Success);
        }

        public Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }
            return Task.FromResult(role.Id.ToString());
        }

        public Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }            
            return Task.FromResult(role.Name);
        }

        public Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }
            role.Name = roleName;
            return Task.FromResult(0);
        }

	    public Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (roleId == null)
            {
                throw new ArgumentNullException("roleId");
            }
            //TRole role = db.GetCollection<TRole>(collectionName).FindOne((Query.EQ("_id", ObjectId.Parse(roleId))));
            TRole role = Roles.FirstOrDefault(i => i.Id.Equals(roleId));
            return Task.FromResult(role);
        }

        public Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            if (role == null)
                throw new ArgumentNullException("role");

            var result = RoleClaims.Where(rc => rc.Id.Equals(role.Id)).Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList();
            return Task.FromResult((IList<Claim>)result);
        }

        public Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }
            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }
            throw new NotImplementedException();
        }

        public Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

		public virtual Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (role == null)
			{
				throw new ArgumentNullException("role");
			}
			return Task.FromResult(role.NormalizedName);
		}

		public virtual Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (role == null)
			{
				throw new ArgumentNullException("role");
			}
			role.NormalizedName = normalizedName;
			return Task.FromResult(0);
		}
	}
}