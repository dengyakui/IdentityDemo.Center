using System.Collections.Generic;

namespace IdentityDemo.Center.Models
{
    public class InputConsentViewModel
    {
        public IEnumerable<string> ScopesConsented { get; set; }
        public bool RememberConsent { get; set; }
        public string Button { get; set; }
        public string ReturnUrl { get; set; }
        public IEnumerable<ScopeViewModel> IdentityScopes { get; set; }
        public IEnumerable<ScopeViewModel> ResourceScopes { get; set; }
    }
}