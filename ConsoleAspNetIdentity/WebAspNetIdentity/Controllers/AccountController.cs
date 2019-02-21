using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace WebAspNetIdentity.Controllers
{
    public class AccountController : Controller
    {
        //This logic moves becuase of simple injector dependency resolver
        //*********************************************************************************************************************************
        public UserManager<IdentityUser> UserManager =>
            HttpContext.GetOwinContext().Get<UserManager<IdentityUser>>();

        public SignInManager<IdentityUser, string> SignInManager =>
            HttpContext.GetOwinContext().Get<SignInManager<IdentityUser, string>>();
        //*********************************************************************************************************************************

        //For extended user
        //*****************************************************************************
        //public UserManager<ExtendedUser> UserManager =>
        //    HttpContext.GetOwinContext().Get<UserManager<ExtendedUser>>();

        //public SignInManager<ExtendedUser, string> SignInManager =>
        //    HttpContext.GetOwinContext().Get<SignInManager<ExtendedUser, string>>();
        //*****************************************************************************

        //To use simple dependency resolver uncomment this method
        //*********************************************************************************************************************************
        //public UserManager<IdentityUser, string> UserManager;
        //public SignInManager<IdentityUser, string> SignInManager;

        //public AccountController(UserManager<IdentityUser, string> userManager,
        //    SignInManager<IdentityUser, string> signInManager)
        //{
        //    UserManager = userManager;
        //    SignInManager = signInManager;
        //}
        //*********************************************************************************************************************************

        public ActionResult ExternalAuthentication(string provider)
        {
            SignInManager.AuthenticationManager.Challenge(
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("ExternalCallback", new { provider })
                }, provider);

            return new HttpUnauthorizedResult();
        }

        public async Task<ActionResult> ExternalCallback(string provider)
        {
            var loginInfo = await SignInManager.AuthenticationManager.GetExternalLoginInfoAsync();
            var signiStatus = await SignInManager.ExternalSignInAsync(loginInfo, true);

            switch (signiStatus)
            {
                case SignInStatus.Success:
                    return RedirectToAction("Index", "Home");
                default:
                    var user = await UserManager.FindByEmailAsync(loginInfo.Email);
                    if (user != null)
                    {
                        var result = await UserManager.AddLoginAsync(user.Id, loginInfo.Login);
                        if (result.Succeeded)
                        {
                            return await ExternalCallback(provider);
                        }
                    }
                    return View("Error");
            }
        }

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

            var result = await UserManager.PasswordValidator.ValidateAsync(model.Password);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", result.Errors.FirstOrDefault());
                return View(model);
            }


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
                case SignInStatus.LockedOut:
                    var user = await UserManager.FindByNameAsync(model.UserName);
                    if (user != null && await UserManager.CheckPasswordAsync(user, model.Password))
                    {
                        // Reaveal the account is locked just when user enters valid credentials.
                        //***********************************************************************
                        ModelState.AddModelError("", "Account Locked");
                        return View(model);
                    }
                    ModelState.AddModelError("", "Invalid Credentials");
                    return View(model);
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

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            var user = await UserManager.FindByNameAsync(model.Username);

            if (user != null)
            {
                var token = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var resetUrl = Url.Action("PasswordReset", "Account", new { userid = user.Id, token = token }, Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "Password Reset", $"Use link to reset password: {resetUrl}");
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult PasswordReset(string userid, string token)
        {
            return View(new PasswordResetViewModel { UserId = userid, Token = token });
        }

        [HttpPost]
        public async Task<ActionResult> PasswordReset(PasswordResetViewModel model)
        {
            var identityResult = await UserManager.ResetPasswordAsync(model.UserId, model.Token, model.Password);

            if (!identityResult.Succeeded)
            {
                return View("Error");
            }

            return RedirectToAction("Index", "Home");
        }


    }
}