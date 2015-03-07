using Microsoft.AspNet.Identity;
using Microsoft.Framework.DependencyInjection;
using MongoDB.Bson.Serialization;
using System;

namespace MongoDB.AspNet.Identity
{
    public static class MongoDBServiceCollectionExtension
    {
        public static IServiceCollection UseMongoDB<TUser>(this IServiceCollection services, Action<IdentityDbContext> configure) 
            where TUser : IdentityUser
        {
            if (configure == null)
                throw new ArgumentNullException("configure");

            BsonClassMap.LookupClassMap(typeof(ExternalLoginInfo));
            //services.AddTransient<IdentityDbContext<ApplicationUser>, IdentityDbContext<ApplicationUser, MongoDB.AspNet.Identity.IdentityRole, string>>();

            services.AddScoped<IdentityDbContext>(sp => {
                var idbc = new IdentityDbContext();
                configure(idbc);
                return idbc;
            });

            services.AddScoped<IRoleStore<IdentityRole>, RoleStore<IdentityRole>>();
            services.AddScoped<IUserStore<TUser>, UserStore<TUser>>();
            services.AddIdentity<TUser, IdentityRole>();

            return services;
        }
    }
}