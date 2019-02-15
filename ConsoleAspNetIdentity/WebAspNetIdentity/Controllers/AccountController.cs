using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml;

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
            var user = await UserManager.FindByNameAsync(model.UserName);
            if (user != null)
            {
                return RedirectToAction("Index", "Home");
            }
            //*****************************************************************************


            var identityUser = new IdentityUser(model.UserName) { Email = model.UserName };
            var identityResult = await UserManager.CreateAsync(identityUser, model.Password);
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
                var token = await UserManager.GenerateEmailConfirmationTokenAsync(identityUser.Id);
                var confirmUrl = Url.Action("ConfirmEmail", "Account", new { userId = identityUser.Id, token = token }, Request.Url.Scheme);
                await UserManager.SendEmailAsync(identityUser.Id, "Email Confirmation", $"Use link to confirm email: {confirmUrl}");


                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", identityResult.Errors.FirstOrDefault());

            return View(model);
        }

        public async Task<ActionResult> ConfirmEmail(string userid, string token)
        {
            var identityResult = await UserManager.ConfirmEmailAsync(userid, token);

            if (!identityResult.Succeeded)
            {
                return View("Error");
            }

            return RedirectToAction("Index", "Home");
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
                case SignInStatus.RequiresVerification:
                    //Create a user with phone number, phone number confirmed, two factor enabled.
                    //********************************************************************************************
                    return RedirectToAction("ChooseProvider");
                //********************************************************************************************
                default:
                    ModelState.AddModelError("", "Invalid Credentials");
                    return View(model);
            }
        }

        public async Task<ActionResult> ChooseProvider()
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            var providers = await UserManager.GetValidTwoFactorProvidersAsync(userId);

            return View(new ChoosePoviderViewModel { Providers = providers.ToList() });
        }

        [HttpPost]
        public async Task<ActionResult> ChooseProvider(ChoosePoviderViewModel model)
        {
            await SignInManager.SendTwoFactorCodeAsync(model.ChosenProvider);
            return RedirectToAction("TwoFactor", new { provider = model.ChosenProvider });
        }

        public ActionResult TwoFactor(string provider)
        {
            return View(new TwoFactorViewModel { Provider = provider });
        }

        [HttpPost]
        public async Task<ActionResult> TwoFactor(TwoFactorViewModel model)
        {
            var signInStatus = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, true, false);

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