using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TestApplication.Startup))]
namespace TestApplication
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
