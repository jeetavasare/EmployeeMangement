using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EmployeeManagement.Controllers
{
	[AllowAnonymous]
	public class AccountController : Controller
	{
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

		[AllowAnonymous]
        [HttpGet]
		public IActionResult Register()
		{
			return View();
		}

		//[HttpPost]
		//[HttpGet] or
		[AcceptVerbs("Get","Post")]
		[AllowAnonymous]
		public async Task<IActionResult> IsEmailInUse(string email)
		{
			var user = await userManager.FindByEmailAsync(email);
			if (user == null)
			{
				return Json(true);
			}
			else
			{
				return Json($"Email {email} is already in use");
			}
		}


		[AllowAnonymous]
		[HttpPost]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = new ApplicationUser
				{
					UserName = model.Email,
					Email = model.Email,
					City = model.City
				};
				var result = await userManager.CreateAsync(user,model.Password);

				if (result.Succeeded)
				{
					if(signInManager.IsSignedIn(User) && User.IsInRole("Administrator"))
					{
						return RedirectToAction("ListUsers", "Administration");
					}
					await signInManager.SignInAsync(user, isPersistent: false);
					return RedirectToAction("Index", "Home");
				}
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
			}
			return View(model);
		}


		[AllowAnonymous]
		[HttpPost]
		public async Task<IActionResult> Logout()
		{
			await signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}

		[AllowAnonymous]
		[HttpGet]
		public async Task<IActionResult> Login(string? returnUrl)
		{
			LoginViewModel model = new LoginViewModel
			{
				ReturnUrl = returnUrl??"~/",
				ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList(),
			};

			return View(model);
		}


		[HttpPost]
		[AllowAnonymous]
		public IActionResult ExternalLogin(string provider,string returnUrl)
		{
			var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });

			var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
			return new ChallengeResult(provider, properties);
		}


        [AllowAnonymous]
        public async Task<IActionResult>
            ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            LoginViewModel loginViewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins =
                        (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            if (remoteError != null)
            {
                ModelState
                    .AddModelError(string.Empty, $"Error from external provider: {remoteError}");

                return View("Login", loginViewModel);
            }

            // Get the login information about the user from the external login provider
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState
                    .AddModelError(string.Empty, "Error loading external login information.");

                return View("Login", loginViewModel);
            }



                // Get the email claim value
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
			ApplicationUser user = null;

			if (email != null)
			{
				user = await userManager.FindByEmailAsync(email);
				if (user != null && !user.EmailConfirmed)
				{
					ModelState.AddModelError("", "Email not confirmed yet");
					return View("Login", loginViewModel);
				}
			}
            // If the user already has a login (i.e if there is a record in AspNetUserLogins
            // table) then sign-in the user with this external login provider
            var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider,
                info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            // If there is no record in AspNetUserLogins table, the user may not have
            // a local account
            else
            {

                if (email != null)
                {
                    // Create a new user without password if we do not have a user already

                    if (user == null)
                    {
                        user = new ApplicationUser
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                        };

                        await userManager.CreateAsync(user);
                    }

                    // Add a login (i.e insert a row for the user in AspNetUserLogins table)
                    await userManager.AddLoginAsync(user, info);
					
                    await signInManager.SignInAsync(user, isPersistent: false);

                    return LocalRedirect(returnUrl);
                }

                // If we cannot find the user email we cannot continue
                ViewBag.ErrorTitle = $"Email claim not received from: {info.LoginProvider}";
                ViewBag.ErrorMessage = "Please open a issue on github.com/jeetavasare/EmployeeMangement";

                return View("Error");
            }
        }

        [AllowAnonymous]
		[HttpPost]
		public async Task<IActionResult> Login(LoginViewModel model, string? ReturnUrl)
		{
			model.ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

			if (ModelState.IsValid)
			{
				var user = await userManager.FindByEmailAsync(model.Email);
				if (user!=null && !user.EmailConfirmed && (await userManager.CheckPasswordAsync(user, model.Password)))
				{
					ModelState.AddModelError("", "Email not confirmed for this account");
					return View(model);
				}
				var result = await signInManager.PasswordSignInAsync(model.Email, model.Password,model.RememberMe,false);
				if (result.Succeeded)
				{
					if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
					{
						//return LocalRedirect(ReturnUrl); This will throw exception if redirect is not local instead check for it in ig statement
						return Redirect(ReturnUrl);
					}
					else
					{
						return RedirectToAction("Index", "Home");
					}
				}
				ModelState.AddModelError("", "Invalid Login Attempt");
				return View(model);
            }
			return View(model);
		}

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
