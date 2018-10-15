using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityDemo.Center.Models;
using IdentityDemo.Center.Services;

namespace IdentityDemo.Center.Controllers
{
    public class ConsentController : Controller
    {
        private readonly IResourceStore _resourceStore;
        private readonly IClientStore _clientStore;
        private readonly IIdentityServerInteractionService _identityServerInteractionService;
        private readonly ConsentService _consentService;

        public ConsentController(IResourceStore resourceStore, IClientStore clientStore, IIdentityServerInteractionService identityServerInteractionService,
           ConsentService consentService)
        {
            _resourceStore = resourceStore;
            _clientStore = clientStore;
            _identityServerInteractionService = identityServerInteractionService;
            this._consentService = consentService;
        }
        public async Task<IActionResult> Index(string returnUrl)
        {
            var context = await _identityServerInteractionService.GetAuthorizationContextAsync(returnUrl);
            var client = await _clientStore.FindEnabledClientByIdAsync(context.ClientId);
            var resources = await _resourceStore.FindEnabledResourcesByScopeAsync(client.AllowedScopes);
            var model =_consentService.CreateConsentViewModel(context, client, resources);
            model.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(InputConsentViewModel model)
        {
            ConsentResponse response = null;
            if (model.Button == "no")
            {
                response = ConsentResponse.Denied;
            }
            else if (model.Button == "yes")
            {
                if (model.ScopesConsented?.Any() ?? false)
                {
                    response = new ConsentResponse()
                    {
                        ScopesConsented = model.ScopesConsented,
                        RememberConsent = model.RememberConsent
                    };
                }
            }

            if (response != null)
            {
                var request = await _identityServerInteractionService.GetAuthorizationContextAsync(model.ReturnUrl);
                await _identityServerInteractionService.GrantConsentAsync(request, response);
                return Redirect(model.ReturnUrl);
            }

            throw new Exception("error");

        }
    }
}