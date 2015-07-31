// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RoleStore.cs" company="">
//
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace MongoDB.AspNet.Identity
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using Microsoft.AspNet.Identity;

    using MongoDB.Bson;
    using MongoDB.Driver;

    /// <summary>
    /// The role store.
    /// </summary>
    /// <typeparam name="TRole">
    /// The type of the role.
    /// </typeparam>
    public class RoleStore<TRole> : RoleStore<TRole, string, IdentityUserRole>, IQueryableRoleStore<TRole>
        where TRole : IdentityRole, new()
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="RoleStore{TRole}" /> class.
        /// </summary>
        public RoleStore()
            : base(new IdentityDbContext().Database)
        {
        }

        #endregion

        #region IDisposable Support

        /// <summary>
        /// The disposed value.
        /// </summary>
        private bool disposedValue; // To detect redundant calls

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                this.disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RoleStore() {
        // // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        // Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);

            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }

    /// <summary>
    /// The role store.
    /// </summary>
    /// <typeparam name="TRole">
    /// The type of the role.
    /// </typeparam>
    /// <typeparam name="TKey">
    /// The type of the key.
    /// </typeparam>
    /// <typeparam name="TUserRole">
    /// The type of the user role.
    /// </typeparam>
    public class RoleStore<TRole, TKey, TUserRole> : IQueryableRoleStore<TRole, TKey>
        where TRole : IdentityRole<TKey, TUserRole>, new() where TUserRole : IdentityUserRole<TKey>, new()
    {
        #region Constants

        /// <summary>
        ///     The collection name.
        /// </summary>
        private const string CollectionName = "AspNetRoles";

        #endregion

        #region Fields

        /// <summary>
        ///     The database.
        /// </summary>
        private readonly IMongoDatabase database;

        /// <summary>
        /// The disposed.
        /// </summary>
        private bool disposed;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleStore{TRole,TKey,TUserRole}"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public RoleStore(IMongoDatabase context)
        {
            this.database = context;
        }

        #endregion

        #region Explicit Interface Properties

        /// <summary>
        ///     Gets the roles.
        /// </summary>
        /// <value>The roles.</value>
        IQueryable<TRole> IQueryableRoleStore<TRole, TKey>.Roles => this.GetRoles().Result;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Creates the specified <paramref name="role"/>.
        /// </summary>
        /// <param name="role">
        /// The role.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The role cannot be null.
        /// </exception>
        public virtual async Task CreateAsync([NotNull] TRole role)
        {
            this.ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            await this.database.GetCollection<TRole>(CollectionName).InsertOneAsync(role).ConfigureAwait(false);
        }

        /// <summary>
        /// The delete async.
        /// </summary>
        /// <param name="role">
        /// The role.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public virtual async Task DeleteAsync([NotNull] TRole role)
        {
            this.ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            await
                this.database.GetCollection<TRole>(CollectionName)
                    .DeleteOneAsync(Builders<TRole>.Filter.Eq("_id", ObjectId.Parse(role.Id.ToString())))
                    .ConfigureAwait(false);
        }

        #region Implementation of IDisposable

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.disposed = true;
        }

        #endregion

        /// <summary>
        /// The find by id async.
        /// </summary>
        /// <param name="roleId">
        /// The role id.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<TRole> FindByIdAsync([NotNull] TKey roleId)
        {
            this.ThrowIfDisposed();

            var cursor =
                await
                this.database.GetCollection<TRole>(CollectionName).FindAsync(Builders<TRole>.Filter.Eq("_id", ObjectId.Parse(roleId.ToString()))).ConfigureAwait(false);
            var list = await cursor.ToListAsync().ConfigureAwait(false);
            var role = list.FirstOrDefault();

            return role;
        }

        /// <summary>
        /// The find by name async.
        /// </summary>
        /// <param name="roleName">
        /// The role name.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<TRole> FindByNameAsync([NotNull] string roleName)
        {
            this.ThrowIfDisposed();

            var cursor = await this.database.GetCollection<TRole>(CollectionName).FindAsync(Builders<TRole>.Filter.Eq("Name", roleName)).ConfigureAwait(false);
            var list = await cursor.ToListAsync().ConfigureAwait(false);
            var role = list.FirstOrDefault();

            return role;
        }

        /// <summary>
        /// The update async.
        /// </summary>
        /// <param name="role">
        /// The role.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public virtual async Task UpdateAsync([NotNull] TRole role)
        {
            this.ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            await
                this.database.GetCollection<TRole>(CollectionName)
                    .ReplaceOneAsync(Builders<TRole>.Filter.Eq("_id", ObjectId.Parse(role.Id.ToString())), role, new UpdateOptions { IsUpsert = true })
                    .ConfigureAwait(false);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Gets the roles from the <see cref="database" />.
        /// </summary>
        /// <returns>The roles.</returns>
        private async Task<IQueryable<TRole>> GetRoles()
        {
            var collection = this.database.GetCollection<TRole>(CollectionName);
            var cursor = await collection.FindAsync(Builders<TRole>.Filter.Where(_ => true)).ConfigureAwait(false);
            var list = await cursor.ToListAsync().ConfigureAwait(false);

            return list.AsQueryable();
        }

        /// <summary>
        ///     The throw if disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// </exception>
        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        #endregion
    }
}
