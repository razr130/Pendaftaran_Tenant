using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(E_Commerce_MultiTenant.Startup))]
namespace E_Commerce_MultiTenant
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
