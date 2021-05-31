using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace StaffClass
{
    class Employee
    {
        public string Employee_id { get; set; }
        public string Name { get; set; }
        public string Hire_date { get; set; }
        public string Position { get; set; }
        public string Base_salary { get; set; }
        public string Login { get; set; }

        public string Premium
        {
            get
            {
                return CountPremium(Hire_date, Position).ToString();
            }
            set
            {
            }
        }


        public Employee() { }
        public Employee(string Employee_id, string Name, string Hire_date, string Position, string Base_salary, string Login) 
        {
            this.Employee_id = Employee_id;
            this.Name = Name;
            this.Hire_date = DateTime.ParseExact(Hire_date, "dd.M.yyyy", null).ToString();
            this.Position = Position;
            this.Base_salary = Base_salary;
            this.Login = Login;
            
        }
        
        public double CountPremium(string date, string position)
        {
            double premium;
           
            DateTime today = DateTime.Now;
            DateTime hire = Convert.ToDateTime(date);

            TimeSpan ts = today - hire;
            double days = (ts).TotalDays / 365;
            int years = (int)Math.Floor(days);

            switch (position)
            {
                case "Employee":
                    premium = Convert.ToDouble(this.Base_salary) * 0.03 * years;
                    break;
                case "Manager":
                    premium = Convert.ToDouble(this.Base_salary) * 0.05 * years;
                    break;
                case "Salesman":
                    premium = Convert.ToDouble(this.Base_salary) * 0.01 * years;
                    break;
                default:
                    premium = 0;
                    break;
            }

            return premium;
        }
    }
}
