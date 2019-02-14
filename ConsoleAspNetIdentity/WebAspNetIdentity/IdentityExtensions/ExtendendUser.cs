using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

namespace WebAspNetIdentity.IdentityExtensions
{
    public class ExtendedUser : IdentityUser
    {
        public ExtendedUser()
        {
            Addresses = new List<AddressModel>();
        }

        public string FullName { get; set; }
        public virtual ICollection<AddressModel> Addresses { get; private set; }
    }
}
