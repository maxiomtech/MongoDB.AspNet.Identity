// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IdentityUser.cs" company="">
//
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace MongoDB.AspNet.Identity
{
    using System;
    using System.Collections.Generic;

    using Microsoft.AspNet.Identity;

    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    /// <summary>
    ///     Class IdentityUser.
    /// </summary>
    public class IdentityUser : IdentityUser<string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>, IUser, IUser<string>
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="IdentityUser" /> class.
        /// </summary>
        public IdentityUser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityUser"/> class.
        /// </summary>
        /// <param name="userName">
        /// Name of the user.
        /// </param>
        public IdentityUser(string userName)
            : this()
        {
            this.UserName = userName;
        }

        #endregion
    }

    /// <summary>
    /// The identity user class.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of the key.
    /// </typeparam>
    /// <typeparam name="TLogin">
    /// The type of the login.
    /// </typeparam>
    /// <typeparam name="TRole">
    /// The type of the role.
    /// </typeparam>
    /// <typeparam name="TClaim">
    /// The type of the claim.
    /// </typeparam>
    public class IdentityUser<TKey, TLogin, TRole, TClaim> : IUser<TKey>
        where TLogin : IdentityUserLogin<TKey> where TRole : IdentityUserRole<TKey> where TClaim : IdentityUserClaim<TKey>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityUser{TKey,TLogin,TRole,TClaim}"/> class.
        ///     Initializes a new instance of the <see cref="IdentityUser"/> class.
        /// </summary>
        public IdentityUser()
        {
            // this.Id = Guid.NewGuid().ToString();
            this.Claims = new List<IdentityUserClaim>();
            this.Roles = new List<string>();
            this.Logins = new List<UserLoginInfo>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the access failed count.
        /// </summary>
        /// <value>
        ///     The access failed count.
        /// </value>
        public virtual int AccessFailedCount { get; set; }

        /// <summary>
        ///     Gets the claims (roles). Matches the AspNet <see cref="IdentityUser"/> entity signature.
        /// </summary>
        /// <value>
        ///     The roles. The claims.
        /// </value>
        public virtual List<IdentityUserClaim> Claims { get; private set; }

        /// <summary>
        ///     Gets or sets the email.
        /// </summary>
        /// <value>
        ///     The email.
        /// </value>
        public virtual string Email { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [email confirmed].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [email confirmed]; otherwise, <c>false</c> .
        /// </value>
        public virtual bool EmailConfirmed { get; set; }

        /// <summary>
        ///     Gets or sets the unique key for the user. <typeparamref name="TKey" /> must be a string.
        /// </summary>
        /// <value>
        ///     The identifier.
        /// </value>
        /// <returns>
        ///     The unique key for the user
        /// </returns>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public virtual TKey Id { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [lockout enabled].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [lockout enabled]; otherwise, <c>false</c> .
        /// </value>
        public virtual bool LockoutEnabled { get; set; }

        /// <summary>
        ///     Gets or sets the lockout end date UTC.
        /// </summary>
        /// <value>
        ///     The lockout end date UTC.
        /// </value>
        public virtual DateTime? LockoutEndDateUtc { get; set; }

        /// <summary>
        ///     Gets the logins.
        /// </summary>
        /// <value>
        ///     The logins.
        /// </value>
        public virtual List<UserLoginInfo> Logins { get; private set; }

        /// <summary>
        ///     Gets or sets the password hash.
        /// </summary>
        /// <value>
        ///     The password hash.
        /// </value>
        public virtual string PasswordHash { get; set; }

        /// <summary>
        ///     Gets or sets the phone number.
        /// </summary>
        /// <value>
        ///     The phone number.
        /// </value>
        public virtual string PhoneNumber { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [phone number confirmed].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [phone number confirmed]; otherwise, <c>false</c> .
        /// </value>
        public virtual bool PhoneNumberConfirmed { get; set; }

        /// <summary>
        ///     Gets the roles. Extended from the AspNet <see cref="IdentityUser"/> entity to add
        ///     a Role array to the users to follow a more Mongo document model
        ///     style.
        /// </summary>
        /// <value>
        ///     The roles.
        /// </value>
        public virtual List<string> Roles { get; private set; }

        /// <summary>
        ///     Gets or sets the security stamp.
        /// </summary>
        /// <value>
        ///     The security stamp.
        /// </value>
        public virtual string SecurityStamp { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [two factor enabled].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [two factor enabled]; otherwise, <c>false</c> .
        /// </value>
        public virtual bool TwoFactorEnabled { get; set; }

        /// <summary>
        ///     Gets or sets the name of the user.
        /// </summary>
        /// <value>
        ///     The name of the user.
        /// </value>
        public virtual string UserName { get; set; }

        #endregion
    }
}
