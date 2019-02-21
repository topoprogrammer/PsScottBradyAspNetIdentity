namespace WebAspNetIdentity.Controllers
{
    public class PasswordResetViewModel
    {
        public string Token { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
    }
}