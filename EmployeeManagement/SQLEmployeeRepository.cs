using EmployeeManagement.Models;

namespace EmployeeManagement
{
    public class SQLEmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext context;

        public SQLEmployeeRepository(AppDbContext context)
        {
            this.context = context;
        }
        public Employee Add(Employee employee)
        {
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
            return employeeToBeDeleted;

        }

        public IEnumerable<Employee> GetAllEmployees()
        {
            return context.Employees;
        }

        public Employee GetEmployee(int id) { 
        
            return context.Employees.Find(id); ;
        }

        public Employee Update(Employee employeeChanges)
        {
            var updatedEmployee = context.Employees.Attach(employeeChanges);
            updatedEmployee.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            context.SaveChanges();
            return employeeChanges;
        }
    }
}
