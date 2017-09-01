using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SmartLinks.Startup))]
namespace SmartLinks
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {

        }
    }
}
