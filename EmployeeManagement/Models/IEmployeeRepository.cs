namespace EmployeeManagement.Models
{
    public interface IEmployeeRepository
    {
        Employee GetEmployee(int Id);
        IEnumerable<Employee> GetAllEmployees();
        Employee Add(Employee employee);
        Employee Update(Employee employeeChanges);
        Employee Delete(int id);
    }
}
