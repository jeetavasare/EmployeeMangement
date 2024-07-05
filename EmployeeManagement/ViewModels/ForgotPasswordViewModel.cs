using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.ViewModels
{
    public class ForgotPasswordViewModel
    {
        public ForgotPasswordViewModel()
        {
            Email = string.Empty;
        }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
