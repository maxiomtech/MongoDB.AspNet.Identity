using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace MongoDB.AspNet.Identity
{


    public class RoleStore<TRole> : RoleStore<TRole, string, IdentityUserRole>, IQueryableRoleStore<TRole>, IQueryableRoleStore<TRole, string>, IRoleStore<TRole, string>, IDisposable
    where TRole : IdentityRole, new()
    {
        public RoleStore() : base(new IdentityDbContext().db) { }
    }

    public class RoleStore<TRole, TKey, TUserRole> : IQueryableRoleStore<TRole, TKey>, IRoleStore<TRole, TKey>, IDisposable
        where TRole : IdentityRole<TKey, TUserRole>, new()
        where TUserRole : IdentityUserRole<TKey>, new()
    {
        private bool _disposed;
        
        private readonly MongoDatabase db;
        private const string collectionName = "AspNetRoles";


        public RoleStore(MongoDatabase context)
        {
            db = context;
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


        public virtual async Task CreateAsync(TRole role)
        {
            this.ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }

            db.GetCollection<TRole>(collectionName).Insert(role);

        }

        public virtual async Task DeleteAsync(TRole role)
        {
            this.ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }

            db.GetCollection(collectionName).Remove((Query.EQ("_id", ObjectId.Parse(role.Id.ToString()))));
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

        public Task<TRole> FindByIdAsync(TKey roleId)
        {
            this.ThrowIfDisposed();
            TRole role = db.GetCollection<TRole>(collectionName).FindOne((Query.EQ("_id", ObjectId.Parse(roleId.ToString()))));
            return Task.FromResult(role);
        }

        public Task<TRole> FindByNameAsync(string roleName)
        {
            this.ThrowIfDisposed();
            TRole role = db.GetCollection<TRole>(collectionName).FindOne((Query.EQ("Name", roleName)));
            return Task.FromResult(role);
        }

        private void ThrowIfDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        public virtual async Task UpdateAsync(TRole role)
        {
            this.ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }

            db.GetCollection<TRole>(collectionName).Update(Query.EQ("_id", ObjectId.Parse(role.Id.ToString())), Update.Replace(role), UpdateFlags.Upsert);


        }
    }
}