using System.Collections.Generic;

namespace WebAspNetIdentity.Controllers
{
    public class ChoosePoviderViewModel
    {
        public List<string> Providers { get; set; }
        public string ChosenProvider { get; set; }
    }
}