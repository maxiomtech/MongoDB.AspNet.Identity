using System;
using System.Configuration;
using System.Linq;
using MongoDB.Driver;

namespace MongoDB.AspNet.Identity
{
    public class IdentityDbContext : IdentityDbContext<IdentityUser>//IdentityDbContext<IdentityUser, IdentityRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>

    {
        public IdentityDbContext() : base() { } //: this("DefaultConnection") { }
        //public IdentityDbContext(string nameOrConnectionString) : base(nameOrConnectionString) { }
    }

    public class IdentityDbContext<TUser> : IdentityDbContext<TUser, IdentityRole, string>//IdentityDbContext<TUser, IdentityRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>
where TUser : IdentityUser
    {
        public IdentityDbContext() : base() { } //: this("DefaultConnection") { }
        //public IdentityDbContext(string nameOrConnectionString) : base(nameOrConnectionString) { }
    }

    public class IdentityDbContext<TUser, TRole, TKey>  : IDisposable
        where TKey : IEquatable<TKey>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        //where TUserLogin : IdentityUserLogin<TKey>
        //where TUserRole : IdentityUserRole<TKey>
        //where TUserClaim : IdentityUserClaim<TKey>
    {

        private MongoDatabase db;

        public MongoDatabase Database { get { return db ?? GetDatabase(); } }

        public string ConnectionString { get; set; }

        /// <summary>
        ///     Gets the database from connection string.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>MongoDatabase.</returns>
        /// <exception cref="System.Exception">No database name specified in connection string</exception>
        private MongoDatabase GetDatabaseFromSqlStyle(string connectionString)
        {
            var conString = new MongoConnectionStringBuilder(connectionString);
            MongoClientSettings settings = MongoClientSettings.FromConnectionStringBuilder(conString);
            MongoServer server = new MongoClient(settings).GetServer();
            if (conString.DatabaseName == null)
            {
                throw new Exception("No database name specified in connection string");
            }
            return server.GetDatabase(conString.DatabaseName);
        }

        /// <summary>
        ///     Gets the database from URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>MongoDatabase.</returns>
        private MongoDatabase GetDatabaseFromUrl(MongoUrl url)
        {
            var client = new MongoClient(url);
            MongoServer server = client.GetServer();
            if (url.DatabaseName == null)
            {
                throw new Exception("No database name specified in connection string");
            }
            return server.GetDatabase(url.DatabaseName); // WriteConcern defaulted to Acknowledged
        }

        /// <summary>
        ///     Uses connectionString to connect to server and then uses databae name specified.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="dbName">Name of the database.</param>
        /// <returns>MongoDatabase.</returns>
        private MongoDatabase GetDatabase(string connectionString, string dbName)
        {
            var client = new MongoClient(connectionString);
            MongoServer server = client.GetServer();
            return server.GetDatabase(dbName);
        }

        private MongoDatabase GetDatabase()
        {
            if (string.IsNullOrEmpty(ConnectionString))
                throw new InvalidOperationException("connection string not provided");

            if (ConnectionString.ToLower().StartsWith("mongodb://"))
            {
                db = GetDatabaseFromUrl(new MongoUrl(ConnectionString));
            }
            else
            {
                db = GetDatabaseFromSqlStyle(ConnectionString);
                ////todo change this to configuration string
                //string connStringFromManager = "Server=localhost:27017;Database=aspnet";
                ////ConfigurationManager.ConnectionStrings[nameOrConnectionString].ConnectionString;
                //if (connStringFromManager.ToLower().StartsWith("mongodb://"))
                //{
                //    db = GetDatabaseFromUrl(new MongoUrl(connStringFromManager));
                //}
                //else
                //{
                //    db = GetDatabaseFromSqlStyle(ConnectionString);
                //}
            }

            return db;
        }

        public bool RequireUniqueEmail
        {
            get;
            set;
        }

        public virtual IQueryable<TRole> Roles
        {
            get;
            set;
        }

        public virtual IQueryable<TUser> Users
        {
            get;
            set;
        }

        public IdentityDbContext()//: this("DefaultConnection") { }
        { }
        //public IdentityDbContext(string nameOrConnectionString)
        //{

        //}

        public void Dispose()
        {
            //Database.Settings.
        }
    }
}