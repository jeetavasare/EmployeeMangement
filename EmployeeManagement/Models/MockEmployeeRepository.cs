namespace EmployeeManagement.Models
{
    public class MockEmployeeRepository : IEmployeeRepository
    {

        private List<Employee> _employeeList;

        public MockEmployeeRepository()
        {
            _employeeList = new List<Employee>()
            {
                new Employee(){Id=1,Name="Jeet", Email="ja@gmail.com",Department=Dept.IT},
                new Employee(){Id=2,Name="SJK", Email="sk@gmail.com",Department=Dept.IT}
            };


        }

        public IEnumerable<Employee> GetAllEmployees()
        {
            return _employeeList;
        }


        public Employee GetEmployee(int id)
        {
            return _employeeList.FirstOrDefault(e => e.Id == id);
        }

        public Employee Add(Employee employee)
        {
            employee.Id = _employeeList.Max(e => e.Id) + 1;
            _employeeList.Add(employee);
            return employee;
        }

        public Employee Update(Employee employeeChanges)
        {
            Employee employeeToBeUpdated = _employeeList.FirstOrDefault(e => e.Id == employeeChanges.Id);
            if (employeeToBeUpdated != null)
            {
                employeeToBeUpdated.Name = employeeChanges.Name;
                employeeToBeUpdated.Email = employeeChanges.Email;
                employeeToBeUpdated.Department = employeeChanges.Department;
            }
            return employeeToBeUpdated;
        }

        public Employee Delete(int Id)
        {
            Employee employeeToBeDeleted = _employeeList.FirstOrDefault(e => e.Id == Id);
            if (employeeToBeDeleted != null)
            {
                _employeeList.Remove(employeeToBeDeleted);
            }
            return employeeToBeDeleted;
        }
    }
}
