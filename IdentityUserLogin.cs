namespace MongoDB.AspNet.Identity
{
    /// <summary>
    /// Represents a login and its associated provider for a user.
    /// </summary>
    public class IdentityUserLogin
    {
        /// <summary>
        /// Gets or sets the login provider (e.g., google, facebook, twitter).
        /// </summary>
        public virtual string LoginProvider { get; set; } = null!;

        /// <summary>
        /// Gets or sets the unique identifier for this login provided by the login provider.
        /// </summary>
        public virtual string ProviderKey { get; set; } = null!;

        /// <summary>
        /// Gets or sets the friendly name used in a UI for this login.
        /// </summary>
        public virtual string? ProviderDisplayName { get; set; }
    }
}
