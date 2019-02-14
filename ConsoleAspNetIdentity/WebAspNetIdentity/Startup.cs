using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

[assembly: OwinStartup(typeof(WebAspNetIdentity.Startup))]

namespace WebAspNetIdentity
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888

            const string connectionString =
                @"Data Source=.\sqlexpress;Initial Catalog=AspIdentityDbWeb;Integrated Security=True";

            app.CreatePerOwinContext(() => new IdentityDbContext(connectionString));
            app.CreatePerOwinContext<UserStore<IdentityUser>>((opt, cont) =>
                new UserStore<IdentityUser>(cont.Get<IdentityDbContext>()));
            app.CreatePerOwinContext<UserManager<IdentityUser>>(
                (opt, cont) => new UserManager<IdentityUser>(cont.Get<UserStore<IdentityUser>>()));
            app.CreatePerOwinContext<SignInManager<IdentityUser, string>>(
                (opt, cont) => new SignInManager<IdentityUser, string>(cont.Get<UserManager<IdentityUser>>(), cont.Authentication));

            //For extended user, defaults are changed 
            //************************************************************************************************************************
            //app.CreatePerOwinContext(() => new ExtendendUserDbContext(connectionString));
            //app.CreatePerOwinContext<UserStore<ExtendedUser>>((opt, cont) =>
            //    new UserStore<ExtendedUser>(cont.Get<ExtendendUserDbContext>()));
            //app.CreatePerOwinContext<UserManager<ExtendedUser>>(
            //    (opt, cont) => new UserManager<ExtendedUser>(cont.Get<UserStore<ExtendedUser>>()));
            //app.CreatePerOwinContext<SignInManager<ExtendedUser, string>>(
            //    (opt, cont) => new SignInManager<ExtendedUser, string>(cont.Get<UserManager<ExtendedUser>>(), cont.Authentication));
            //************************************************************************************************************************

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
            });

        }
    }
}
