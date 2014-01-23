using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace MongoDB.AspNet.Identity
{
    /// <summary>
    /// Class IdentityUser.
    /// </summary>
    public class IdentityUser : IUser
    {
        /// <summary>
        /// Unique key for the user
        /// </summary>
        /// <value>The identifier.</value>
        /// <returns>The unique key for the user</returns>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
	    public virtual string Id { get; set; }
        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>The name of the user.</value>
		public virtual string UserName { get; set; }
        /// <summary>
        /// Gets or sets the password hash.
        /// </summary>
        /// <value>The password hash.</value>
		public virtual string PasswordHash { get; set; }
        /// <summary>
        /// Gets or sets the security stamp.
        /// </summary>
        /// <value>The security stamp.</value>
		public virtual string SecurityStamp { get; set; }
        /// <summary>
        /// Gets the roles.
        /// </summary>
        /// <value>The roles.</value>
		public virtual List<string> Roles { get; private set; }
        /// <summary>
        /// Gets the claims.
        /// </summary>
        /// <value>The claims.</value>
		public virtual List<IdentityUserClaim> Claims { get; private set; }
        /// <summary>
        /// Gets the logins.
        /// </summary>
        /// <value>The logins.</value>
		public virtual List<UserLoginInfo> Logins { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityUser"/> class.
        /// </summary>
		public IdentityUser()
		{
			this.Claims = new List<IdentityUserClaim>();
			this.Roles = new List<string>();
			this.Logins = new List<UserLoginInfo>();
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityUser"/> class.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
		public IdentityUser(string userName) : this()
		{
			this.UserName = userName;
		}
	}

    /// <summary>
    /// Class IdentityUserClaim.
    /// </summary>
	public class IdentityUserClaim
	{
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public virtual string Id { get; set; }
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        public virtual string UserId { get; set; }
        /// <summary>
        /// Gets or sets the type of the claim.
        /// </summary>
        /// <value>The type of the claim.</value>
        public virtual string ClaimType { get; set; }
        /// <summary>
        /// Gets or sets the claim value.
        /// </summary>
        /// <value>The claim value.</value>
		public virtual string ClaimValue { get; set; }
        
	}
}
