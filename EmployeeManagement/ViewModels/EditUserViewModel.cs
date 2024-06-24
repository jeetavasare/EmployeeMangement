using EmployeeManagement.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.ViewModels
{
	public class EditUserViewModel
	{
        public EditUserViewModel()
        {
            //Initializing so we don't get null reference exception in the view
            Claims = new List<string>();
            Roles= new List<string>();
        }

        public string Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string City { get; set; }
        public List<string> Claims { get; set; }
        public IList<string> Roles{ get; set; }


    }
}
