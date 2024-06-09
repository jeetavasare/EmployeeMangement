using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Models
{
    public class SQLEmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext context;
        private readonly ILogger logger;

        public SQLEmployeeRepository(AppDbContext context,ILogger<SQLEmployeeRepository> logger)
        {
            this.context = context;
            this.logger = logger;
        }
        public Employee Add(Employee employee)
        {
            logger.LogInformation($"New Employee Added:\n ID: {employee.Id}, Name: {employee.Name} on {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            context.Employees.Add(employee);
            context.SaveChanges();
            return employee;
        }

        public Employee Delete(int id)
        {
            Employee employeeToBeDeleted = context.Employees.Find(id);
            if (employeeToBeDeleted != null)
            {
                context.Employees.Remove(employeeToBeDeleted);
                context.SaveChanges();
            }
            logger.LogInformation($"Employee Deleted:\n ID: {id} on {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            return employeeToBeDeleted;

        }

        public IEnumerable<Employee> GetAllEmployees()
        {
            return context.Employees;
        }

        public Employee GetEmployee(int id)
        {
            logger.LogInformation($"Employee with id {id} visited");
            return context.Employees.Find(id); ;
        }

        public Employee Update(Employee employeeChanges)
        {
            var updatedEmployee = context.Employees.Attach(employeeChanges);
            updatedEmployee.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            context.SaveChanges();
            logger.LogInformation($"Employee Updated:\n ID: {employeeChanges.Id}, Name: {employeeChanges.Name} on {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            return employeeChanges;
        }
    }
}
