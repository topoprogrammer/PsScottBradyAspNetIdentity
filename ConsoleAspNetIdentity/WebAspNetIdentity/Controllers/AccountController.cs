using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace WebAspNetIdentity.Controllers
{
    public class AccountController : Controller
    {
        public UserManager<IdentityUser> UserManager =>
            HttpContext.GetOwinContext().Get<UserManager<IdentityUser>>();

        public SignInManager<IdentityUser, string> SignInManager =>
            HttpContext.GetOwinContext().Get<SignInManager<IdentityUser, string>>();

        //For extended user
        //*****************************************************************************
        //public UserManager<ExtendedUser> UserManager =>
        //    HttpContext.GetOwinContext().Get<UserManager<ExtendedUser>>();

        //public SignInManager<ExtendedUser, string> SignInManager =>
        //    HttpContext.GetOwinContext().Get<SignInManager<ExtendedUser, string>>();
        //*****************************************************************************

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            // Don't show that the account exists because is an account enumeration risk.
            // Ideally give a generic error (not telling that the user exists) and
            // email the user informing they already have an account and they don't need
            // to register again.
            //*****************************************************************************
            var identityUser = await UserManager.FindByNameAsync(model.UserName);
            if (identityUser != null)
            {
                return RedirectToAction("Index", "Home");
            }
            //*****************************************************************************



            var identityResult = await UserManager.CreateAsync(new IdentityUser(model.UserName), model.Password);
            //For extendend user
            //********************************************************************************************
            //var user = new ExtendedUser
            //{
            //    UserName = model.UserName,
            //    FullName = model.FullName,
            //};

            //user.Addresses.Add(new AddressModel
            //{
            //    AddressLine = model.AddressLine,
            //    Country = model.Country,
            //    UserId = user.Id
            //});
            //var identityResult = await UserManager.CreateAsync(user, model.Password);

            //********************************************************************************************

            if (identityResult.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", identityResult.Errors.FirstOrDefault());

            return View(model);
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            var signInStatus = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, true, true);

            switch (signInStatus)
            {
                case SignInStatus.Success:
                    return RedirectToAction("Index", "Home");
                default:
                    ModelState.AddModelError("", "Invalid Credentials");
                    return View(model);
            }

        }
    }
}