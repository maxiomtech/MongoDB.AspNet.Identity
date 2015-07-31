// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IdentityDbContext.cs" company="">
//
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace MongoDB.AspNet.Identity
{
    using System;
    using System.Configuration;
    using System.Linq;

    using MongoDB.Driver;

    /// <summary>
    /// The identity database context.
    /// </summary>
    /// <typeparam name="TUser">
    /// The type of the user.
    /// </typeparam>
    public class IdentityDbContext<TUser> : IdentityDbContext<TUser, IdentityRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>
        where TUser : IdentityUser
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityDbContext{TUser}"/> class.
        /// </summary>
        public IdentityDbContext()
            : this(Constants.DefaultConnection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityDbContext{TUser}"/> class.
        /// </summary>
        /// <param name="nameOrConnectionString">
        /// The name or connection string.
        /// </param>
        public IdentityDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        #endregion
    }

    /// <summary>
    ///     The identity database context.
    /// </summary>
    public class IdentityDbContext : IdentityDbContext<IdentityUser, IdentityRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityDbContext"/> class.
        /// </summary>
        public IdentityDbContext()
            : this(Constants.DefaultConnection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityDbContext"/> class.
        /// </summary>
        /// <param name="nameOrConnectionString">
        /// The name or connection string.
        /// </param>
        public IdentityDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        #endregion
    }

    /// <summary>
    /// The identity database context.
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
    public class IdentityDbContext<TUser, TRole, TKey, TUserLogin, TUserRole, TUserClaim> : IDisposable
        where TUser : IdentityUser<TKey, TUserLogin, TUserRole, TUserClaim>
        where TRole : IdentityRole<TKey, TUserRole>
        where TUserLogin : IdentityUserLogin<TKey>
        where TUserRole : IdentityUserRole<TKey>
        where TUserClaim : IdentityUserClaim<TKey>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityDbContext{TUser,TRole,TKey,TUserLogin,TUserRole,TUserClaim}"/> class.
        /// </summary>
        public IdentityDbContext()
            : this(Constants.DefaultConnection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityDbContext{TUser,TRole,TKey,TUserLogin,TUserRole,TUserClaim}"/> class.
        /// </summary>
        /// <param name="nameOrConnectionString">
        /// The name or connection string.
        /// </param>
        public IdentityDbContext(string nameOrConnectionString)
        {
            MongoUrl mongoUrl = null;
            if (nameOrConnectionString.StartsWith("mongodb://", StringComparison.OrdinalIgnoreCase))
            {
                mongoUrl = new MongoUrl(nameOrConnectionString);
            }
            else
            {
                var connStringFromManager = ConfigurationManager.ConnectionStrings[nameOrConnectionString].ConnectionString;
                if (connStringFromManager.StartsWith("mongodb://", StringComparison.OrdinalIgnoreCase))
                {
                    mongoUrl = new MongoUrl(connStringFromManager);
                }
            }

            if (mongoUrl?.DatabaseName == null)
            {
                throw new Exception("No database name specified in connection string or invalid connection string.");
            }

            this.Database = new MongoClient(mongoUrl).GetDatabase(mongoUrl.DatabaseName); // WriteConcern defaulted to Acknowledged
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the database.
        /// </summary>
        /// <value>
        ///     The database.
        /// </value>
        public IMongoDatabase Database { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether to require a unique email
        ///     address.
        /// </summary>
        /// <value>
        ///     A value indicating whether to require a unique email address.
        /// </value>
        public bool RequireUniqueEmail { get; set; }

        /// <summary>
        ///     Gets or sets the roles.
        /// </summary>
        /// <value>
        ///     The roles.
        /// </value>
        public virtual IQueryable<TRole> Roles { get; set; }

        /// <summary>
        ///     Gets or sets the users.
        /// </summary>
        /// <value>
        ///     The users.
        /// </value>
        public virtual IQueryable<TUser> Users { get; set; }

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
        /// The <paramref name="disposing"/>.
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
        // ~IdentityDbContext() {
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
}
