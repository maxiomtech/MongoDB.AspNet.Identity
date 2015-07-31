// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IdentityUserRole.cs" company="">
//
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace MongoDB.AspNet.Identity
{
    /// <summary>
    /// The identity user role.
    /// </summary>
    public class IdentityUserRole : IdentityUserRole<string>
    {
    }

    /// <summary>
    /// The identity user role.
    /// </summary>
    /// <typeparam name="TKey">
    /// </typeparam>
    public class IdentityUserRole<TKey>
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the role id.
        /// </summary>
        public virtual TKey RoleId { get; set; }

        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public virtual TKey UserId { get; set; }

        #endregion
    }
}