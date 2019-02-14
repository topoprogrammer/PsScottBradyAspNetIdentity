using System;

namespace WebAspNetIdentity.IdentityExtensions
{
    public class AddressModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public string AddressLine { get; set; }
        public string Country { get; set; }
    }
}