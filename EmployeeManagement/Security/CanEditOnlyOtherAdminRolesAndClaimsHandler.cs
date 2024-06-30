using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace EmployeeManagement.Security
{
    public class CanEditOnlyOtherAdminRolesAndClaimsHandler : AuthorizationHandler<ManageAdminRolesAndClaimsRequiement>
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public CanEditOnlyOtherAdminRolesAndClaimsHandler(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ManageAdminRolesAndClaimsRequiement requirement)
        {
            //var authFilterContext = context.Resource as AuthorizationFilterContext;
            //if(authFilterContext == null)
            //{
            //    return Task.CompletedTask;  
            //}

            string loggedInAdminId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            string adminIdBeingEdited = httpContextAccessor.HttpContext.Request.Query["userId"];

            if((context.User.IsInRole("Administrator")
                && context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true")
                && adminIdBeingEdited.ToLower() != loggedInAdminId.ToLower()) || context.User.IsInRole("SuperAdmin"))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
