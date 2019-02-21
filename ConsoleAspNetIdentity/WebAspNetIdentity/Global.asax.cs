using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using SimpleInjector;
using SimpleInjector.Integration.Web;
using SimpleInjector.Integration.Web.Mvc;
using System;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebAspNetIdentity.Services;

namespace WebAspNetIdentity
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //To use simple dependency resolver uncomment this method
            //************************************************************************************************************************
            //ConfigureContainer();
            //************************************************************************************************************************
        }

        private static void ConfigureContainer()
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new WebRequestLifestyle();

            const string connectionString =
                @"Data Source=.\sqlexpress;Initial Catalog=AspIdentityDbWeb;Integrated Security=True";

            container.Register(() => new IdentityDbContext(connectionString), Lifestyle.Scoped);
            container.Register(() => new UserStore<IdentityUser>(container.GetInstance<IdentityDbContext>()), Lifestyle.Scoped);
            container.Register(() =>
            {
                var userManager = new UserManager<IdentityUser, string>(container.GetInstance<UserStore<IdentityUser>>());
                userManager.RegisterTwoFactorProvider("SMS", new PhoneNumberTokenProvider<IdentityUser> { MessageFormat = "Token: {0}" });
                userManager.SmsService = new SmsService();
                //DataProtectorTokenProvider is tightly coupled in opt.DataProtectionProvider.Create()
                //userManager.UserTokenProvider = new DataProtectorTokenProvider<IdentityUser, string>(opt.DataProtectionProvider.Create());
                userManager.EmailService = new EmailService();

                userManager.UserValidator = new UserValidator<IdentityUser, string>(userManager) { RequireUniqueEmail = true };
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

            }, Lifestyle.Scoped);

            container.Register<SignInManager<IdentityUser, string>>(Lifestyle.Scoped);

            container.Register(() => container.IsVerifying
            ? new OwinContext().Authentication
            : HttpContext.Current.GetOwinContext().Authentication,
            Lifestyle.Scoped);

            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());

            container.Verify();

            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
        }
    }
}
