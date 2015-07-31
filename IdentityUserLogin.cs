// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IdentityUserLogin.cs" company="">
//
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace MongoDB.AspNet.Identity
{
    /// <summary>
    /// The identity user login.
    /// </summary>
    public class IdentityUserLogin : IdentityUserLogin<string>
    {
    }

    /// <summary>
    /// The identity user login.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of the key.
    /// </typeparam>
    public class IdentityUserLogin<TKey>
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the login provider.
        /// </summary>
        public virtual string LoginProvider { get; set; }

        /// <summary>
        /// Gets or sets the provider key.
        /// </summary>
        public virtual string ProviderKey { get; set; }

        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public virtual TKey UserId { get; set; }

        #endregion
    }
}