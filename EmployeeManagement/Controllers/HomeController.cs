using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IWebHostEnvironment _webhostenvironment;

        public HomeController(IEmployeeRepository employeeRepository, IWebHostEnvironment webHostEnvironment)
        {
            _employeeRepository = employeeRepository;
            _webhostenvironment = webHostEnvironment;
        }


		//[Route("")]
		//[Route("Home")]
		//[Route("Home/Index")]
		public ViewResult Index()
        {
            return View(_employeeRepository.GetAllEmployees());
            //return _employeeRepository.GetEmployee(1).Name;
            //return Json(new { id = 1, name = "Jeet" });
            //return "From Controller:Home->Index()";
        }

        [Route("[action]/{id?}")]
        public ViewResult Details(int? id)
        {
            Employee model = _employeeRepository.GetEmployee(id??1);

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
                    string uploadsFolder = Path.Combine(_webhostenvironment.WebRootPath, "images");
                    uniqueFilename = Guid.NewGuid().ToString()+ "_" + model.Photo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFilename);

                    //model.Photo.CopyTo(new FileStream(filePath,FileMode.Create));
                    //FileStream fs = new FileStream(filePath, FileMode.Create);
                    //model.Photo.CopyTo(fs);
                    //fs.Close();
                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
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
    }
}
