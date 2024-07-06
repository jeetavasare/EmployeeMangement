using EmployeeManagement.Models;
using EmployeeManagement.Security;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Cryptography;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EmployeeManagement.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IWebHostEnvironment _webhostenvironment;
        private readonly IDataProtector _dataProtector;
        public HomeController(IEmployeeRepository employeeRepository, IWebHostEnvironment webHostEnvironment,
            IDataProtectionProvider dataProtectionProvider, DataProtectionPurposeStrings dataProtectionPurposeStrings)
        {
            _employeeRepository = employeeRepository;
            _webhostenvironment = webHostEnvironment;
            _dataProtector = dataProtectionProvider.CreateProtector(dataProtectionPurposeStrings.EmployeeIdRouteValue);
        }


        //[Route("")]
        //[Route("Home")]
        //[Route("Home/Index")]
        [AllowAnonymous]
		public ViewResult Index()
        {
            var model = _employeeRepository.GetAllEmployees().Select(e =>
            {
                e.EncryptedId = _dataProtector.Protect(e.Id.ToString());
                return e;
            });
            return View(model);
            //return _employeeRepository.GetEmployee(1).Name;
            //return Json(new { id = 1, name = "Jeet" });
            //return "From Controller:Home->Index()";
        }

        [AllowAnonymous]
        [Route("[action]/{id?}")]
        public ViewResult Details(string? id)
        {
            int decryptedId;
            try
            {
                decryptedId = Convert.ToInt32(_dataProtector.Unprotect(id));
            }
            catch(CryptographicException)
            {
                return View("NotFound");
            }
            Employee model = _employeeRepository.GetEmployee(decryptedId);
            if (model == null)
            {
                Response.StatusCode = 404;
                return View("EmployeeNotFound",decryptedId);
            }
            //ViewData["Employee"] = result;
            //ViewData["PageTitle"] = "Employee Details";

            //ViewBag.Employee = result;
            //ViewBag.PageTitle = "Employee Details";

            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel()
            {
                Employee = model,
                PageTitle = "Employee Details"
            };
            return View(homeDetailsViewModel);
            
        }

        [HttpGet]
        public ViewResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(EmployeeCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFilename = "";
                if (model.Photo != null)
                {
                    string uploadsFolder = Path.Combine(_webhostenvironment.WebRootPath, "images/userprofiles");
                    uniqueFilename = Guid.NewGuid().ToString()+ "_" + model.Photo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFilename);

                    //model.Photo.CopyTo(new FileStream(filePath,FileMode.Create)); //method1

                    //FileStream fs = new FileStream(filePath, FileMode.Create); //method2
                    //model.Photo.CopyTo(fs);
                    //fs.Close();
                    using (FileStream fs = new FileStream(filePath, FileMode.Create)) //method3
                    {
                        model.Photo.CopyTo(fs);
                    }
                }
                Employee newEmployee = new Employee
                {
                    Name = model.Name,
                    Email = model.Email,
                    Department = model.Department,
                    PhotoPath = uniqueFilename
                };
                _employeeRepository.Add(newEmployee);
                return RedirectToAction("Details", new { id = newEmployee.Id });
            }
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public ViewResult Edit(int id)
        {
            Employee employeeTobeEdited = _employeeRepository.GetEmployee(id);
            EmployeeEditViewModel employeeEditViewModelObj = new EmployeeEditViewModel
            {
                Id = employeeTobeEdited.Id,
                Name = employeeTobeEdited.Name,
                Email = employeeTobeEdited.Email,
                Department = employeeTobeEdited.Department,
                ExistingPhotoPath = employeeTobeEdited.PhotoPath
            };
            return View(employeeEditViewModelObj);
        }

        [HttpPost]
        public IActionResult Edit(EmployeeEditViewModel updatedEmployee)
        {
            if (ModelState.IsValid)
            {
                Employee employee = _employeeRepository.GetEmployee(updatedEmployee.Id);
                employee.Name = updatedEmployee.Name;
                employee.Email = updatedEmployee.Email;
                employee.Department = updatedEmployee.Department;

                if(updatedEmployee.Photo != null)
                {
                    if(updatedEmployee.ExistingPhotoPath != null)
                    {
                        string existingprofile = Path.Combine(_webhostenvironment.WebRootPath, "images/userprofiles", updatedEmployee.ExistingPhotoPath);
                        System.IO.File.Delete(existingprofile);
                    }
                    employee.PhotoPath = WriteUploadedProfileToImages(updatedEmployee);
                }

                _employeeRepository.Update(employee);

                return RedirectToAction("Details", new { id = updatedEmployee.Id });
            }
            return View();
        }

        private string WriteUploadedProfileToImages(EmployeeEditViewModel updatedEmployee)
        {
            string uniqueFilename = "";
            if (updatedEmployee.Photo != null)
            {
                string uploadsFolder = Path.Combine(_webhostenvironment.WebRootPath, "images/userprofiles");
                uniqueFilename = Guid.NewGuid().ToString() + "_" + updatedEmployee.Photo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFilename);
                using (FileStream fs = new FileStream(filePath, FileMode.Create)) //method3
                {
                    updatedEmployee.Photo.CopyTo(fs);
                }
            }

            return uniqueFilename;
        }
    }
}
