using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace IdentityDemo.Center.Controllers
{
    public class ConsentController : Controller
    {
        private readonly IResourceStore _resourceStore;
        private readonly IClientStore _clientStore;
        private readonly IIdentityServerInteractionService _identityServerInteractionService;
         private async Task<ConsentViewModel> BuildConsentViewModelAsync(string returnUrl)
        {
            var resquest = await _identityServerInteractionService.GetAuthorizationContextAsync(returnUrl);
            if (resquest == null) return null;
            var client = await _clientStore.FindEnabledClientByIdAsync(resquest.ClientId);
            var resources = await _resourceStore.FindEnabledResourcesByScopeAsync(resquest.ScopesRequested);

            var vm = CreateConsentViewModel(resquest, client, resources);
            vm.ReturnUrl = returnUrl;
            return vm;
        }

        private ConsentViewModel CreateConsentViewModel(AuthorizationRequest request, Client client, Resources resources)
        {
            var vm = new ConsentViewModel();
            vm.ClientId = client.ClientId;
            vm.ClientName = client.ClientName;
            vm.ClientLogoUrl = client.LogoUri;
            vm.ClientUrl = client.ClientUri;

            vm.IdentityScopes = resources.IdentityResources.Select(i => CreateScopeViewModel(i));
            vm.ResourceScopes = resources.ApiResources.SelectMany(i => i.Scopes).Select(i => CreateScopeViewModel(i));
            return vm;
        }

        private ScopeViewModel CreateScopeViewModel(IdentityResource identityResource)
        {
            return new ScopeViewModel
            {
                Name = identityResource.Name,
                Checked = identityResource.Required,
                Required = identityResource.Required,
                Description = identityResource.Description,
                Emphasize = identityResource.Emphasize,
                DisplayName = identityResource.DisplayName
            };
        }

        private ScopeViewModel CreateScopeViewModel(Scope scope)
        {
            return new ScopeViewModel
            {
                Name = scope.Name,
                Checked = scope.Required,
                Required = scope.Required,
                Description = scope.Description,
                Emphasize = scope.Emphasize,
                DisplayName = scope.DisplayName
            };
        }
        public ConsentController(IResourceStore resourceStore, IClientStore clientStore, IIdentityServerInteractionService identityServerInteractionService)
        {
            _resourceStore = resourceStore;
            _clientStore = clientStore;
            _identityServerInteractionService = identityServerInteractionService;
        }
        public async Task<IActionResult> Index(string returnUrl)
        {
            var context = await _identityServerInteractionService.GetAuthorizationContextAsync(returnUrl);
            var client = await _clientStore.FindEnabledClientByIdAsync(context.ClientId);
            var resources = await _resourceStore.FindEnabledResourcesByScopeAsync(client.AllowedScopes);
            var model = CreateConsentViewModel(context, client, resources);
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

    public class ConsentViewModel : InputConsentViewModel
    {
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientLogoUrl { get; set; }
        public string ClientUrl { get; set; }

    }

    public class InputConsentViewModel
    {
        public IEnumerable<string> ScopesConsented { get; set; }
        public bool RememberConsent { get; set; }
        public string Button { get; set; }
        public string ReturnUrl { get; set; }
        public IEnumerable<ScopeViewModel> IdentityScopes { get; set; }
        public IEnumerable<ScopeViewModel> ResourceScopes { get; set; }
    }
    public class ScopeViewModel
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool Emphasize { get; set; }
        public bool Required { get; set; }
        public bool Checked { get; set; }
    }
}