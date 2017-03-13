using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ShoppingSite.Startup))]
namespace ShoppingSite
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
