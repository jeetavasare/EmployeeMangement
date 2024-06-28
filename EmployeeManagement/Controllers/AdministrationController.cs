using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EmployeeManagement.Controllers
{
	[Authorize(Roles = "Administrator")]
	public class AdministrationController : Controller
	{
		private readonly RoleManager<IdentityRole> roleManger;
		private readonly UserManager<ApplicationUser> userManager;
		private readonly ILogger<AdministrationController> logger;

		public AdministrationController(RoleManager<IdentityRole> roleManger, UserManager<ApplicationUser> userManager, ILogger<AdministrationController> logger)
		{
			this.roleManger = roleManger;
			this.userManager = userManager;
			this.logger = logger;
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

		[Authorize(Policy ="EditRolePolicy")]
		[HttpGet]
		public async Task<IActionResult> EditRole(string id)
		{
			var role = await roleManger.FindByIdAsync(id);
			if (role == null)
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
				if (await userManager.IsInRoleAsync(user, role.Name))
				{
					model.Users.Add(user.UserName);
				}
			}
			return View(model);
		}

		[HttpPost]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> EditRole(EditRoleViewModel model, string id)
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

		[HttpGet]
		public async Task<IActionResult> EditUsersInRole(string roleId)
		{
			ViewBag.roleId = roleId;

			var role = await roleManger.FindByIdAsync(roleId);
			if (role == null)
			{
				ViewBag.ErrorMessage = $"The role ({roleId}) could not be found";
				return View("NotFound");
			}

			var model = new List<UserRoleViewModel>();

			foreach (var user in await userManager.Users.ToListAsync())
			{
				var userRoleViewModel = new UserRoleViewModel
				{
					UserId = user.Id,
					UserName = user.UserName
				};

				if (await userManager.IsInRoleAsync(user, role.Name))
				{
					userRoleViewModel.IsSelected = true;
				}
				else
				{
					userRoleViewModel.IsSelected = false;
				}

				model.Add(userRoleViewModel);
			}

			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model, string roleId)
		{
			var role = await roleManger.FindByIdAsync(roleId);
			if (role == null)
			{
				ViewBag.ErrorMessage = $"The role ({roleId}) could not be found";
				return View("NotFound");
			}

			for (int i = 0; i < model.Count; i++)
			{
				var user = await userManager.FindByIdAsync(model[i].UserId);
				IdentityResult? result = null;
				if (model[i].IsSelected && !(await userManager.IsInRoleAsync(user, role.Name)))
				{
					result = await userManager.AddToRoleAsync(user, role.Name);
				}
				else if (!model[i].IsSelected && await userManager.IsInRoleAsync(user, role.Name))
				{
					result = await userManager.RemoveFromRoleAsync(user, role.Name);
				}
				else
				{
					continue;
				}

				if (result.Succeeded)
				{
					if (i < model.Count - 1)
					{
						continue;
					}
					else
					{
						return RedirectToAction("EditRole", new { id = roleId });
					}
				}
			}

			return RedirectToAction("EditRole", new { id = roleId });
		}


		[HttpGet]
		public IActionResult ListUsers()
		{
			var users = userManager.Users;
			return View(users);
		}

		[HttpGet]
		public async Task<IActionResult> EditUsers(string id)
		{
			var user = await userManager.FindByIdAsync(id);
			if (user == null)
			{
				ViewBag.ErrorMessage = $"The user with id:{id} could not be found";
				return View("NotFound");
			}

			var userClaims = await userManager.GetClaimsAsync(user);
			var userRoles = await userManager.GetRolesAsync(user);
			EditUserViewModel userToBeEdited = new EditUserViewModel
			{
				Id = id,
				UserName = user.UserName,
				Email = user.Email,
				City = user.City,
				Claims = userClaims.Select(c => c.Value).ToList(),
				Roles = userRoles
			};

			return View(userToBeEdited);
		}


		[HttpPost]
		public async Task<IActionResult> EditUsers(EditUserViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await userManager.FindByIdAsync(model.Id);
				if (user == null)
				{
					ViewBag.ErrorMessage = $"The user with id:{model.Id} could not be found";
					return View("NotFound");
				}
				else
				{
					user.UserName = model.UserName;
					user.Email = model.Email;
					user.City = model.City;

                    var result = await userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("ListUsers", "Administration");
                    }
					foreach(var error in result.Errors)
					{
						ModelState.AddModelError("",error.Description);
					}

					return View(model);
                }
			}
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> DeleteUser(string id)
		{
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"The user with id:{id} could not be found";
                return View("NotFound");
            }
			else
			{
				var result = await userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListUsers", "Administration");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
				}

                return View("ListUsers");
            }
		}
		
		[HttpPost]
		[Authorize(Policy = "DeleteRolePolicy")]
		public async Task<IActionResult> DeleteRole(string id)
		{
            var role = await roleManger.FindByIdAsync(id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"The role with id:{id} could not be found";
                return View("NotFound");
            }
			else
			{
				try
				{
					var result = await roleManger.DeleteAsync(role);
					if (result.Succeeded)
					{
						return RedirectToAction("ListRoles", "Administration");
					}
					foreach (var error in result.Errors)
					{
						ModelState.AddModelError("", error.Description);
					}

					return View("ListRoles");
				}
				catch (DbUpdateException ex)
				{
					logger.LogError($"{ex.Message} Role {role.Name} is in use");
					logger.LogError($"{ex.InnerException}");
					ViewBag.ErrorTitle = $"Role {role.Name} is in use";
					ViewBag.ErrorMessage = $"Role {role.Name} could not be deleted because there are users which have been assigned to this role. First remove all users from this role and try again";
					return View("Error");
				}
            }
		}

		[HttpGet]
		public async Task<IActionResult> ManageUserRoles(string id)
		{
			ViewBag.userId = id;

			var user = await userManager.FindByIdAsync(id);

			if(user == null)
			{
				ViewBag.ErrorMessage = $"The user with id {id} could not be found";
				return View("NotFound");
			}

			var model = new List<UserRolesViewModel>();
			foreach(var role in await roleManger.Roles.ToListAsync())
			{
				var userRolesViewModel = new UserRolesViewModel
				{
					RoleId = role.Id,
					RoleName = role.Name,
				};
				if(await userManager.IsInRoleAsync(user,role.Name))
				{
					userRolesViewModel.IsSelected = true;
				}
				else
				{
					userRolesViewModel.IsSelected = false;
				}
				model.Add(userRolesViewModel);
			}
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> ManageUserRoles(List<UserRolesViewModel> model, string id)
		{
			var user = await userManager.FindByIdAsync(id);

			if (user == null)
			{
				ViewBag.ErrorMessage = $"User with Id = {id} cannot be found";
				return View("NotFound");
			}

			var roles = await userManager.GetRolesAsync(user);
			var result = await userManager.RemoveFromRolesAsync(user, roles);

			if (!result.Succeeded)
			{
				ModelState.AddModelError("", "Cannot remove user existing roles");
				return View(model);
			}

			result = await userManager.AddToRolesAsync(user,
				model.Where(x => x.IsSelected).Select(y => y.RoleName));

			if (!result.Succeeded)
			{
				ModelState.AddModelError("", "Cannot add selected roles to user");
				return View(model);
			}

			return RedirectToAction("EditUsers", new { Id = id });
		}


		[HttpGet]
		public async Task<IActionResult> ManageUserClaims(string userId)
		{
			var user = await userManager.FindByIdAsync(userId);

			if (user == null)
			{
				ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
				return View("NotFound");
			}

			// UserManager service GetClaimsAsync method gets all the current claims of the user
			var existingUserClaims = await userManager.GetClaimsAsync(user);

			var model = new UserClaimsViewModel
			{
				UserId = userId
			};

			// Loop through each claim we have in our application
			foreach (Claim claim in ClaimsStore.AllClaims)
			{
				UserClaim userClaim = new UserClaim
				{
					ClaimType = claim.Type
				};

				// If the user has the claim, set IsSelected property to true, so the checkbox
				// next to the claim is checked on the UI
				if (existingUserClaims.Any(c => c.Type == claim.Type))
				{
					userClaim.IsSelected = true;
				}

				model.Claims.Add(userClaim);
			}

			return View(model);

		}

        [HttpPost]
        public async Task<IActionResult> ManageUserClaims(UserClaimsViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {model.UserId} cannot be found";
                return View("NotFound");
            }

            // Get all the user existing claims and delete them
            var claims = await userManager.GetClaimsAsync(user);
            var result = await userManager.RemoveClaimsAsync(user, claims);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing claims");
                return View(model);
            }

            // Add all the claims that are selected on the UI
            result = await userManager.AddClaimsAsync(user,
                model.Claims.Where(c => c.IsSelected).Select(c => new Claim(c.ClaimType, c.ClaimType)));

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected claims to user");
                return View(model);
            }

            return RedirectToAction("EditUsers", new { Id = model.UserId });

        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View("AccessDenied");
        }

    }
}
