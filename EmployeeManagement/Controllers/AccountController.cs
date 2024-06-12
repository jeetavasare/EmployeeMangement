﻿using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Controllers
{
	public class AccountController : Controller
	{
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpGet]
		public IActionResult Register()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = new IdentityUser
				{
					UserName = model.Email,
					Email = model.Email
				};
				var result = await userManager.CreateAsync(user,model.Password);

				if (result.Succeeded)
				{
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

		[HttpPost]
		public async Task<IActionResult> Logout()
		{
			await signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}


		[HttpGet]
		public IActionResult Login()
		{
			return View();
		}
		
		[HttpPost]
		public async Task<IActionResult> Login(LoginViewModel model, string ReturnUrl)
		{
			if (ModelState.IsValid)
			{
				var result = await signInManager.PasswordSignInAsync(model.Email, model.Password,model.RememberMe,false);
				if (result.Succeeded)
				{
					if (!string.IsNullOrEmpty(ReturnUrl))
					{
						return Redirect(ReturnUrl);
					}
					else
					{
						return RedirectToAction("Index", "Home");
					}
				}
				ModelState.AddModelError("", "Invalid Username of password");
				return View(model);
            }
			return View(model);
		}
	}
}
