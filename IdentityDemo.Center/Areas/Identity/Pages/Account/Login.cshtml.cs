﻿using IdentityServer4.Services;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Events;

namespace IdentityDemo.Center.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IEventService _eventService;

        private readonly IIdentityServerInteractionService _identityServerInteractionService;
        //private readonly TestUserStore _testUserStore;

        public LoginModel(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger, IIdentityServerInteractionService identityServerInteractionService, IEventService eventService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _identityServerInteractionService = identityServerInteractionService;
            _eventService = eventService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                //var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    ModelState.AddModelError(nameof(user), "用户不存在");
                }
                else
                {
                    var result = await _userManager.CheckPasswordAsync(user, Input.Password);
                    if (result)
                    {
                        _logger.LogInformation("User logged in.");
                        var props = new AuthenticationProperties
                            {IsPersistent = Input.RememberMe, ExpiresUtc = DateTimeOffset.Now.AddMinutes(30)};
                        await _signInManager.SignInAsync(user, props);

                        if (_identityServerInteractionService.IsValidReturnUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }

                        return Redirect("~/");

                    }
                    else
                    {
                        await _eventService.RaiseAsync(new UserLoginFailureEvent(user.UserName, "invalid credentials"));
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return Page();
                    }
                }



            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
