using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaffClass
{
    class Employee
    {
        public int Employee_id { get; set; }
        public string Name { get; set; }
        public string Hire_date { get; set; }
        public string Position { get; set; }
        public int Base_salary { get; set; }
        public string Login { get; set; }

        public Employee() { }
        public Employee(int Employee_id, string Name, string Hire_date, string Position, int Base_salary, string Login) 
        {
            this.Employee_id = Employee_id;
            this.Name = Name;
            this.Hire_date = Hire_date;
            this.Position = Position;
            this.Base_salary = Base_salary;
            this.Login = Login;
        }
    }
}
