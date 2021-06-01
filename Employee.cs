using System;
using System.Collections.Generic;
using System.Data.SQLite;
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
                return CountPremium(Position).ToString();
            }
            set
            {
            }
        }
        public double SubordinatesSummSalary
        {
            get
            {
                return getSummSalaryOfSubordinates();
            }
            set { }
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

        // Запрос к бд
        private SQLiteDataReader dataTableQuerry(string dataBaseFileName, string querry)
        {
            SQLiteConnection dbConnection;
            SQLiteCommand sqlCommand = new SQLiteCommand();
            dbConnection = new SQLiteConnection($"Data Source={dataBaseFileName};Version=3;");
            dbConnection.Open();
            sqlCommand.Connection = dbConnection;
            sqlCommand.CommandText = querry;
            SQLiteDataReader reader = sqlCommand.ExecuteReader();
            return reader;
        }


        // Возвращает сумму зарплаты подчинённых
        private double getSummSalaryOfSubordinates()
        {
            // Поиск id подчинённых
            SQLiteDataReader reader = dataTableQuerry("base.db", $"SELECT Subordinate_id FROM Bosses WHERE Chief_id = {Employee_id};");
            double summSalary = 0;

            // Список с id подчинённых
            List<object> subordinateIdList = new List<object>();
            while (reader.Read())
            {
                subordinateIdList.Add(reader["Subordinate_id"]);
            }

            // Рассчёт суммарной зарплаты подчинённых
            foreach (object id in subordinateIdList)
            {
                // Находим зарплату подчинённого
                reader = dataTableQuerry("base.db", $"SELECT Base_salary FROM Staff WHERE Employee_id = {id};");
                
                // Находим суммарную зарплату
                while (reader.Read())
                {
                    summSalary += Convert.ToDouble(reader["Base_salary"]);
                }
            }
            
            // НАДО К СУММАРНЫЙ ЗАРПЛАТЕ ДОБАВИТЬ ПРЕМИИ, А ОНИ ХРАНЯТСЯ НЕ В БАЗЕ ДАННЫХ, А РАССЧИТЫВАЮТСЯ В КОДЕ
            return summSalary;
        }

        // Рассчёт опыта работы
        private int getWorkExperience(string date)
        {
            DateTime today = DateTime.Now;
            DateTime hire = Convert.ToDateTime(date);

            TimeSpan ts = today - hire;
            double days = (ts).TotalDays / 365;

            int years = (int)Math.Floor(days);

            return years;
        }
        

        
        // Подсчёт премии сотрудника
        private double CountPremium(string position)
        {
            double premium;
            double salary = Convert.ToDouble(this.Base_salary);
            int years = getWorkExperience(this.Hire_date);


            // К премии manager'у и salesman'у надо добавить %суммарной зарплаты всех подчинённых
            switch (position)
            {
                case "Employee":
                    premium = years * 0.03;
                    if (premium > 0.3)
                    {
                        premium = salary * 0.3;
                    }
                    else
                    {
                        premium *= salary;
                    }

                    break;
                
                case "Manager":
                    premium = years* 0.05;
                    if (premium > 0.4)
                    {
                        premium = salary * 0.4;
                    }
                    else
                    {
                        premium *= salary;
                    }
                    break;
                
                case "Salesman":
                    premium = years * 0.01;
                    if (premium > 0.35)
                    {
                        premium = salary * 0.35;
                    }
                    else
                    {
                        premium *= salary;
                    }
                    break;

                default:
                    premium = 0;
                    break;
            }

            return premium;
        }

        // Сюда поместить расчёт выработанных лет из CountPremium
        private int CountWorkExperience() 
        {
            return 0;
        }
    }
}
