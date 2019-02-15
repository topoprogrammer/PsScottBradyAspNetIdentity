using Microsoft.AspNet.Identity;
using System.Threading.Tasks;

namespace WebAspNetIdentity.Services
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            return Task.CompletedTask;
        }
    }
}
