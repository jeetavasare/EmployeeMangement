using Microsoft.EntityFrameworkCore;
using System.Security.Permissions;

namespace EmployeeManagement.Models
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().HasData(
                    new Employee
                    {
                        Id = 1,
                        Name = "Jeet",
                        Email = "jeetubhai@gmail.com",
                        Department = Dept.IT
                    },
                    new Employee
                    {
                        Id = 2,
                        Name = "SJK",
                        Email = "sj@gmail.com",
                        Department = Dept.IT
                    }
                );
        }
    
    }
}
