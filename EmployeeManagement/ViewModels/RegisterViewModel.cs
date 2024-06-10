using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.ViewModels
{
	public class RegisterViewModel
	{
		[Required]
		[EmailAddress]
        public string Email { get; set; }

		[Required]
		[DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
		[DataType(DataType.Password)]
		[DisplayName("Confirm Password")]
		[Compare("Password", ErrorMessage = "Password and Confirm Password do not match")]
        public string ConfirmPassword { get; set; }
    }
}
