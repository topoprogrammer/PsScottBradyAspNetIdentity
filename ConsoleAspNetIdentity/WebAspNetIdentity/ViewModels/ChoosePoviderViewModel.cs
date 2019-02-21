using System.Collections.Generic;

namespace WebAspNetIdentity.ViewModels
{
    public class ChoosePoviderViewModel
    {
        public List<string> Providers { get; set; }
        public string ChosenProvider { get; set; }
    }
}