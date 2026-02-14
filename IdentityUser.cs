using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.AspNet.Identity
{
    /// <summary>
    /// Represents a user in the identity system.
    /// </summary>
    public class IdentityUser
    {
        /// <summary>
        /// Gets or sets the unique identifier for this user.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public virtual string Id { get; set; } = null!;

        /// <summary>
        /// Gets or sets the user name for this user.
        /// </summary>
        public virtual string? UserName { get; set; }

        /// <summary>
        /// Gets or sets the normalized user name for this user.
        /// </summary>
        public virtual string? NormalizedUserName { get; set; }

        /// <summary>
        /// Gets or sets the email address for this user.
        /// </summary>
        public virtual string? Email { get; set; }

        /// <summary>
        /// Gets or sets the normalized email address for this user.
        /// </summary>
        public virtual string? NormalizedEmail { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the email address has been confirmed.
        /// </summary>
        public virtual bool EmailConfirmed { get; set; }

        /// <summary>
        /// Gets or sets the password hash for this user.
        /// </summary>
        public virtual string? PasswordHash { get; set; }

        /// <summary>
        /// Gets or sets a random value that should change when a user's credentials change.
        /// </summary>
        public virtual string? SecurityStamp { get; set; }

        /// <summary>
        /// Gets or sets a random value that should change when a user is persisted to the store.
        /// </summary>
        public virtual string? ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the phone number for this user.
        /// </summary>
        public virtual string? PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the phone number has been confirmed.
        /// </summary>
        public virtual bool PhoneNumberConfirmed { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if two factor authentication is enabled for this user.
        /// </summary>
        public virtual bool TwoFactorEnabled { get; set; }

        /// <summary>
        /// Gets or sets the date and time when any user lockout ends.
        /// </summary>
        public virtual DateTimeOffset? LockoutEnd { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if lockout is enabled for this user.
        /// </summary>
        public virtual bool LockoutEnabled { get; set; }

        /// <summary>
        /// Gets or sets the number of failed login attempts for this user.
        /// </summary>
        public virtual int AccessFailedCount { get; set; }

        /// <summary>
        /// Gets the roles for this user.
        /// </summary>
        public virtual List<string> Roles { get; private set; } = new List<string>();

        /// <summary>
        /// Gets the claims for this user.
        /// </summary>
        public virtual List<IdentityUserClaim> Claims { get; private set; } = new List<IdentityUserClaim>();

        /// <summary>
        /// Gets the logins for this user.
        /// </summary>
        public virtual List<IdentityUserLogin> Logins { get; private set; } = new List<IdentityUserLogin>();

        /// <summary>
        /// Gets the authentication tokens for this user.
        /// </summary>
        public virtual List<IdentityUserToken> Tokens { get; private set; } = new List<IdentityUserToken>();

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityUser"/> class.
        /// </summary>
        public IdentityUser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityUser"/> class with the specified user name.
        /// </summary>
        /// <param name="userName">The user name.</param>
        public IdentityUser(string userName) : this()
        {
            UserName = userName;
        }
    }
}
