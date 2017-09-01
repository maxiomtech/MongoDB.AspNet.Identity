MongoDB.AspNet.Identity
=======================

ASP.NET Identity provider that users MongoDB for storage

## Purpose ##

ASP.NET MVC 5 shipped with a new Identity system (in the Microsoft.AspNet.Identity.Core package) in order to support both local login and remote logins via OpenID/OAuth, but only ships with an
Entity Framework provider (Microsoft.AspNet.Identity.EntityFramework).

## News ##
__02-11-2014__ - [http://blogs.msdn.com/b/webdev/archive/2014/02/11/announcing-preview-of-microsoft-aspnet-identity-2-0-0-beta1.aspx](Microsoft has released Microsoft.AspNet.Identity v2 Beta 1). I will be addressing these issues and introducing them into the MongoDB.AspNet.Identity provider.

## Features ##
* Drop-in replacement ASP.NET Identity with MongoDB as the backing store.
* Requires only 1 mongo document type, while EntityFramework requires 5 tables
* Contains the same IdentityUser class used by the EntityFramework provider in the MVC 5 project template.
* Supports additional profile properties on your application's user model.
* Provides UserStore<TUser> implementation that implements the same interfaces as the EntityFramework version:
    * IUserStore<TUser>
    * IUserLoginStore<TUser>
    * IUserRoleStore<TUser>
    * IUserClaimStore<TUser>
    * IUserPasswordStore<TUser>
    * IUserSecurityStampStore<TUser>
    * IUserEmailStore<TUser> (1.0.7)
    * IUserLockoutStore<TUser, string> (1.0.7)
    * IUserTwoFactorStore<TUser, string> (1.0.7)

## Instructions ##
These instructions assume you know how to set up MongoDB within an MVC application.

1. Create a new ASP.NET MVC 5 project, choosing the Individual User Accounts authentication type.
2. Remove the Entity Framework packages and replace with MongoDB Identity:

```PowerShell
Uninstall-Package Microsoft.AspNet.Identity.EntityFramework
Uninstall-Package EntityFramework
Install-Package MongoDB.AspNet.Identity
```
    
3. In ~/Models/IdentityModels.cs:
    * Remove the namespace: Microsoft.AspNet.Identity.EntityFramework
    * Add the namespace: MongoDB.AspNet.Identity
	* Remove the ApplicationDbContext class completely.
4. In ~/Controllers/AccountController.cs
    * Remove the namespace: Microsoft.AspNet.Identity.EntityFramework
    * Add the connection string name to the constructor of the UserStore. Or empty constructor will use DefaultConnection
5. In ~/App_Start/IdentityConfig.cs
    * Remove reference to ApplicationDbContext
    * Amend ApplicationUserManager to inherit UserManager<ApplicationUser, string>. Also amend parameter for ApplicationUser.GenerateUserIdentityAsync
6. In ~/App_Start/Startup.Auth.cs
    * Remove app.CreatePerOwinContext<ApplicationDbContext>(ApplicationDbContext.Create);

```C#
AccountController requires a parameterless constructor. 
You could instantiate the UserManager in this contructor using any the UserStore constructors.
public AccountController()
{
    //examples
    this.UserManager = 
    new ApplicationUserManager(new UserStore<ApplicationUser>("MyConnection"));
}
```

## Connection Strings ##
The UserStore has multiple constructors for handling connection strings. Here are some examples of the expected inputs and where the connection string should be located.

### 1. SQL Style ###
```C#
UserStore<TUser>(string connectionNameOrUrl)
```
<code>UserStore<TUser>("Mongo")</code>

**web.config**
```xml
<add name="Mongo" connectionString="Server=localhost:27017;Database={YourDataBase}" />
```

### 2. Mongo Style ###
```C#
UserStore<TUser>(string connectionNameOrUrl)
```
<code>UserStore<TUser>("Mongo")</code>

**web.config**
```xml
<add name="Mongo" connectionString="mongodb://localhost/{YourDataBase}" />
```

**OR**

```C#
UserStore<TUser>(string connectionNameOrUrl)
```
<code>UserStore<TUser>("mongodb://localhost/{YourDataBase}")</code>


## Thanks To ##

Special thanks to [David Boike](https://github.com/DavidBoike) whos [RavenDB AspNet Identity](https://github.com/ILMServices/RavenDB.AspNet.Identity) project gave me the base for jumpstarting the MongoDB provider
