using EmployeeManagement.Models;
using EmployeeManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;

        public HomeController(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
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
                PageTitle = "EmployeeDetails"
            };

            return View(homeDetailsViewModel);
            
        }

        [HttpGet]
        public ViewResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                Employee newEmployee = _employeeRepository.Add(employee);
                return RedirectToAction("Details", new { id = newEmployee.Id });
            }
            return View();
        }
    }
}
