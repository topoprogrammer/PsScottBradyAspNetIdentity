using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using System;
using System.Configuration;
using WebAspNetIdentity.Services;

[assembly: OwinStartup(typeof(WebAspNetIdentity.Startup))]

namespace WebAspNetIdentity
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //This logic is not neccesary becuase it is resolved with simple injector dependency resolver
            //*********************************************************************************************************************************
            const string connectionString =
                @"Data Source=.\sqlexpress;Initial Catalog=AspIdentityDbWeb;Integrated Security=True";

            app.CreatePerOwinContext(() => new IdentityDbContext(connectionString));
            app.CreatePerOwinContext<UserStore<IdentityUser>>((opt, cont) =>
                new UserStore<IdentityUser>(cont.Get<IdentityDbContext>()));
            app.CreatePerOwinContext<UserManager<IdentityUser>>(
                (opt, cont) =>
                {
                    var userManager = new UserManager<IdentityUser>(cont.Get<UserStore<IdentityUser>>());
                    userManager.RegisterTwoFactorProvider("SMS", new PhoneNumberTokenProvider<IdentityUser> { MessageFormat = "Token: {0}" });
                    userManager.SmsService = new SmsService();
                    userManager.UserTokenProvider = new DataProtectorTokenProvider<IdentityUser, string>(opt.DataProtectionProvider.Create());
                    userManager.EmailService = new EmailService();

                    userManager.UserValidator = new UserValidator<IdentityUser>(userManager) { RequireUniqueEmail = true };
                    userManager.PasswordValidator = new PasswordValidator
                    {
                        RequireDigit = true,
                        RequireLowercase = true,
                        RequireNonLetterOrDigit = true,
                        RequireUppercase = true,
                        RequiredLength = 8
                    };

                    userManager.UserLockoutEnabledByDefault = true;
                    userManager.MaxFailedAccessAttemptsBeforeLockout = 2;
                    userManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(3);

                    return userManager;
                });

            app.CreatePerOwinContext<SignInManager<IdentityUser, string>>(
                (opt, cont) => new SignInManager<IdentityUser, string>(cont.Get<UserManager<IdentityUser>>(), cont.Authentication));
            //*********************************************************************************************************************************

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

            //To use simple dependency resolver uncomment this method
            //*********************************************************************************************************************************
            //app.CreatePerOwinContext<UserManager<IdentityUser, string>>(() => DependencyResolver.Current
            //    .GetService<UserManager<IdentityUser, string>>());
            //*********************************************************************************************************************************

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<UserManager<IdentityUser, string>, IdentityUser>(
                        validateInterval: TimeSpan.FromSeconds(3),
                        regenerateIdentity: (manager, user) => manager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie))
                }
            });

            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions
            {
                ClientId = ConfigurationManager.AppSettings["google:ClientId"],
                ClientSecret = ConfigurationManager.AppSettings["google:ClientSecret"],
                Caption = "Google"
            });

        }
    }
}
