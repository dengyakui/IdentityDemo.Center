using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityDemo.Center.Models;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;

namespace IdentityDemo.Center.Services
{
    public class ConsentService
    {
        private readonly IClientStore _clientStore;
        private readonly IResourceStore _resourceStore;
        private readonly IIdentityServerInteractionService _identityServerInteractionService;

        public ConsentService(IClientStore clientStore, IResourceStore resourceStore, IIdentityServerInteractionService identityServerInteractionService)
        {
            this._clientStore = clientStore;
            this._resourceStore = resourceStore;
            this._identityServerInteractionService = identityServerInteractionService;
        }

        public async Task<ConsentViewModel> BuildConsentViewModelAsync(string returnUrl)
        {
            var resquest = await _identityServerInteractionService.GetAuthorizationContextAsync(returnUrl);
            if (resquest == null) return null;
            var client = await _clientStore.FindEnabledClientByIdAsync(resquest.ClientId);
            var resources = await _resourceStore.FindEnabledResourcesByScopeAsync(resquest.ScopesRequested);

            var vm = CreateConsentViewModel(resquest, client, resources);
            vm.ReturnUrl = returnUrl;
            return vm;
        }

        public ConsentViewModel CreateConsentViewModel(AuthorizationRequest request, Client client, Resources resources)
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

    }
}