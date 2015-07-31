// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IdentityRole.cs" company="">
//
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace MongoDB.AspNet.Identity
{
    using System;
    using System.Collections.Generic;

    using Microsoft.AspNet.Identity;

    /// <summary>
    /// The identity role.
    /// </summary>
    public class IdentityRole : IdentityRole<string, IdentityUserRole>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityRole"/> class.
        /// </summary>
        public IdentityRole()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityRole"/> class.
        /// </summary>
        /// <param name="roleName">
        /// The role name.
        /// </param>
        public IdentityRole(string roleName)
            : this()
        {
            this.Name = roleName;
        }

        #endregion
    }

    /// <summary>
    /// The identity role.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of the key.
    /// </typeparam>
    /// <typeparam name="TUserRole">
    /// The type of the user role.
    /// </typeparam>
    public class IdentityRole<TKey, TUserRole> : IRole<TKey>
        where TUserRole : IdentityUserRole<TKey>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityRole{TKey,TUserRole}"/> class.
        /// </summary>
        public IdentityRole()
        {
            this.Users = new List<TUserRole>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        public TKey Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the users.
        /// </summary>
        /// <value>The users.</value>
        public ICollection<TUserRole> Users { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get users.
        /// </summary>
        /// <returns>
        /// The <see cref="ICollection{TUserRole}"/>.
        /// </returns>
        public virtual ICollection<TUserRole> GetUsers() => this.Users;

        #endregion
    }
}
