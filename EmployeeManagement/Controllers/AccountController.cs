using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Security.Claims;

namespace EmployeeManagement.Controllers
{
	[AllowAnonymous]
	public class AccountController : Controller
	{
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ILogger<AccountController> logger;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
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
					var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

					var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token }, Request.Scheme);
					logger.LogWarning(confirmationLink);

					if(signInManager.IsSignedIn(User) && User.IsInRole("Administrator"))
					{
						return RedirectToAction("ListUsers", "Administration");
					}

					ViewBag.ErrorTitle = "Registration Successful";
					ViewBag.ErrorMessage = $"Before you can login,please confirm your email by clicking the confirmation link emailed to {user.Email}";
					return View("Error");
					//await signInManager.SignInAsync(user, isPersistent: false);
					//return RedirectToAction("Index", "Home");
				}
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
			}
			return View(model);
		}

		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> ConfirmEmail(string userId, string token)
		{
			if(userId == null || token == null)
			{
				return RedirectToAction("Index", "Home");
			}
			var user = await userManager.FindByIdAsync(userId);
			if(user == null)
			{
				//Actually the user with the incoming id is invalid
				ViewBag.ErrorMessage = $"Invalid Application Link";
			}
            var result = await userManager.ConfirmEmailAsync(user, token);
			if (result.Succeeded)
			{
				return View();
			}

			ViewBag.ErrorTitle = "Unable to verify confirmation link";
			return View("Error");
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

						//since this is a new user registering generate his email confirmation link
                        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

                        var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token }, Request.Scheme);
                        logger.LogWarning(confirmationLink);

                        if (signInManager.IsSignedIn(User) && User.IsInRole("Administrator"))
                        {
                            return RedirectToAction("ListUsers", "Administration");
                        }

                        ViewBag.ErrorTitle = "Registration Successful";
                        ViewBag.ErrorMessage = $"Before you can login,please confirm your email by clicking the confirmation link emailed to {user.Email}";
                        return View("Error");


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

		[AllowAnonymous]
		[HttpGet]
		public IActionResult ForgotPassword()
		{
			return View();
		}
		
		[AllowAnonymous]
		[HttpPost]
		public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await userManager.FindByEmailAsync(model.Email);
				if (user == null)
				{
					ViewBag.ErrorTitle = "Coudn't verify email address";
					return View("Error");
				}
				else
				{
					if (user.EmailConfirmed)
					{
						var token = await userManager.GeneratePasswordResetTokenAsync(user);
						var resetPasswordLink = Url.Action("ResetPassword", "Account", new { email = user.Email, token = token }, Request.Scheme);
						logger.LogWarning(resetPasswordLink);
						return View("ForgotPassWordConfirmationView");
					}
				}
			}
			ModelState.AddModelError("", "No email linked to your account yet");
			return View(model);

			
        }

		[AllowAnonymous]
		[HttpGet]
		public async Task<IActionResult> ResetPassword(string? email, string? token)
		{
			if(token == null || email == null)
			{
                ModelState.AddModelError("", "Invalid password reset token");
            }
            return View();

        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Find the user by email
                var user = await userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    // reset the user password
                    var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
                    if (result.Succeeded)
                    {
                        return View("ResetPasswordConfirmation");
                    }
                    // Display validation errors. For example, password reset token already
                    // used to change the password or password complexity rules not met
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }

                // To avoid account enumeration and brute force attacks, don't
                // reveal that the user does not exist
                return View("ResetPasswordConfirmation");
            }
            // Display validation errors if model state is not valid
            return View(model);
        }

		[AllowAnonymous]
		[HttpGet]
		public IActionResult ResetPasswordConfirmation()
		{
			return View();
		}

		[HttpGet]
		public IActionResult ChangePassword()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await userManager.GetUserAsync(User);
				if (user == null)
				{
					return View("Login");
				}
				var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
				if (!result.Succeeded)
				{
					foreach(var error in result.Errors)
					{
						ModelState.AddModelError("",error.Description);
					}
					return View();
				}
				await signInManager.RefreshSignInAsync(user);
				return View("ChangePasswordConfirmation");
			}
			return View(model);
		}
    }
}
