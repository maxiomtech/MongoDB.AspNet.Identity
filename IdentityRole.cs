using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.AspNet.Identity
{
    /// <summary>
    /// Represents a role in the identity system.
    /// </summary>
    public class IdentityRole
    {
        /// <summary>
        /// Gets or sets the unique identifier for this role.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public virtual string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        /// <summary>
        /// Gets or sets the name for this role.
        /// </summary>
        public virtual string? Name { get; set; }

        /// <summary>
        /// Gets or sets the normalized name for this role.
        /// </summary>
        public virtual string? NormalizedName { get; set; }

        /// <summary>
        /// Gets or sets a random value that should change when the role is persisted to the store.
        /// </summary>
        public virtual string? ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityRole"/> class.
        /// </summary>
        public IdentityRole()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityRole"/> class with the specified role name.
        /// </summary>
        /// <param name="roleName">The role name.</param>
        public IdentityRole(string roleName) : this()
        {
            Name = roleName;
        }
    }
}
