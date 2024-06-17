using EmployeeManagement.Controllers;
using EmployeeManagement.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Execution;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.ViewModels
{
	public class RegisterViewModel
	{
		[Required]
		[EmailAddress]
		[Remote(action:"IsEmailInUse",controller:"Account")]
		[ValidEmailDomain(allowedDomain:"jeet.com", ErrorMessage = "Email domain must be jeet.com")]
        public string Email { get; set; }

		[Required]
		[DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
		[DataType(DataType.Password)]
		[DisplayName("Confirm Password")]
		[Compare("Password", ErrorMessage = "Password and Confirm Password do not match")]
        public string ConfirmPassword { get; set; }

		public string City { get; set; }
	}
}
