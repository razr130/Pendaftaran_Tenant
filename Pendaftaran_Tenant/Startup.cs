using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Pendaftaran_Tenant.Startup))]
namespace Pendaftaran_Tenant
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
