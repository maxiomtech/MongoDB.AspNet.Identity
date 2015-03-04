using System;
using System.Collections.Generic;

namespace MongoDB.AspNet.Identity
{
    public class IdentityRole : IdentityRole<string>
    {
        public IdentityRole()
        {
            
        }

        public IdentityRole(string roleName)
            : this()
        {
            base.Name = roleName;
        }
    }


    public class IdentityRole<TKey> where TKey : IEquatable<TKey>
    {
        public IdentityRole() { }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="roleName"></param>
        public IdentityRole(string roleName) : this()
        {
            Name = roleName;
        }

        /// <summary>
        ///     Navigation property for users in the role
        /// </summary>
        public virtual ICollection<IdentityUserRole<TKey>> Users { get; private set; } = new List<IdentityUserRole<TKey>>();

        /// <summary>
        ///     Navigation property for claims in the role
        /// </summary>
        public virtual ICollection<IdentityRoleClaim<TKey>> Claims { get; private set; } = new List<IdentityRoleClaim<TKey>>();

        /// <summary>
        ///     Role id
        /// </summary>
        public virtual TKey Id { get; set; }

        /// <summary>
        ///     Role name
        /// </summary>
        public virtual string Name { get; set; }
		public virtual string NormalizedName { get; set; }
	}
}