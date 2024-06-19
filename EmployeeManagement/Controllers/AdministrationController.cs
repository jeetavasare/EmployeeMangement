using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Controllers
{
	public class AdministrationController : Controller
	{
		private readonly RoleManager<IdentityRole> roleManger;
        private readonly UserManager<ApplicationUser> userManager;

        public AdministrationController(RoleManager<IdentityRole> roleManger, UserManager<ApplicationUser> userManager)
		{
			this.roleManger = roleManger;
            this.userManager = userManager;
        }

		[HttpGet]
		public IActionResult CreateRole()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
		{
			if (ModelState.IsValid)
			{
				IdentityRole role = new IdentityRole
				{
					Name = model.RoleName,
				};

				IdentityResult result = await roleManger.CreateAsync(role);
				if (result.Succeeded)
				{
					return RedirectToAction("ListRoles", "Administration");
				}
				else
				{
					foreach (IdentityError error in result.Errors)
					{
						ModelState.AddModelError("", error.Description);
					}
				}
			}
			return View(model);
		}

		[HttpGet]
		public IActionResult ListRoles()
		{
			var roles = roleManger.Roles;
			return View(roles);
		}

		[HttpGet]
		public async Task<IActionResult> EditRole(string id)
		{
			var role = await roleManger.FindByIdAsync(id);
			if(role == null)
			{
				ViewBag.ErrorMessage = $"The role ({id}) could not be found";
				return View("NotFound");
			}

			var model = new EditRoleViewModel
			{
				Id = id,
				RoleName = role.Name
			};

			foreach (var user in await userManager.Users.ToListAsync())
			{
				if(await userManager.IsInRoleAsync(user, role.Name))
				{
					model.Users.Add(user.UserName);
				}
			}
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> EditRole(EditRoleViewModel model)
		{
			var role = await roleManger.FindByIdAsync(model.Id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"The role ({model.Id}) could not be found";
                return View("NotFound");
            }
			else
			{
				role.Name = model.RoleName;
				var result = await roleManger.UpdateAsync(role);

				if (result.Succeeded)
				{
					return RedirectToAction("ListRoles");
				}

				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
				return View(model);
			}
			
        }
    }
}
