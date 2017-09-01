using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BioData_Update.Startup))]
namespace BioData_Update
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
