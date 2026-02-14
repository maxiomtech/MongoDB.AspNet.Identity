using System.Security.Claims;

namespace MongoDB.AspNet.Identity
{
    /// <summary>
    /// Represents a claim that a user possesses.
    /// </summary>
    public class IdentityUserClaim
    {
        /// <summary>
        /// Gets or sets the claim type for this claim.
        /// </summary>
        public virtual string ClaimType { get; set; } = null!;

        /// <summary>
        /// Gets or sets the claim value for this claim.
        /// </summary>
        public virtual string? ClaimValue { get; set; }

        /// <summary>
        /// Converts the entity into a Claim instance.
        /// </summary>
        /// <returns>A <see cref="Claim"/> representing the claim.</returns>
        public virtual Claim ToClaim()
        {
            return new Claim(ClaimType, ClaimValue ?? string.Empty);
        }

        /// <summary>
        /// Initializes this instance from the specified claim.
        /// </summary>
        /// <param name="claim">The claim to initialize from.</param>
        public virtual void InitializeFromClaim(Claim claim)
        {
            ClaimType = claim.Type;
            ClaimValue = claim.Value;
        }
    }
}
