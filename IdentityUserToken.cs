namespace MongoDB.AspNet.Identity
{
    /// <summary>
    /// Represents an authentication token for a user.
    /// </summary>
    public class IdentityUserToken
    {
        /// <summary>
        /// Gets or sets the login provider this token is from.
        /// </summary>
        public virtual string LoginProvider { get; set; } = null!;

        /// <summary>
        /// Gets or sets the name of the token.
        /// </summary>
        public virtual string Name { get; set; } = null!;

        /// <summary>
        /// Gets or sets the value of the token.
        /// </summary>
        public virtual string? Value { get; set; }
    }
}
