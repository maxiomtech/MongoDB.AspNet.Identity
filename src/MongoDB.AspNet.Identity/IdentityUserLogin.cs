using System;

namespace MongoDB.AspNet.Identity
{
    public class IdentityUserLogin : IdentityUserLogin<string>
    {
        public IdentityUserLogin()
        {
        }
    }

    public class IdentityUserLogin<TKey> where TKey : IEquatable<TKey>
    {
        public virtual string LoginProvider
        {
            get;
            set;
        }

        public virtual string ProviderKey
        {
            get;
            set;
        }

        public virtual TKey UserId
        {
            get;
            set;
        }

        public IdentityUserLogin()
        {
        }
    }
}