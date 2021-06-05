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
        private int WorkExperience
        {
            get
            {
                return getWorkExperience(this.Hire_date);
            }
        }
        private double SubordinatesSummSalary
        {
            get
            {
                return getSummSalaryOfSubordinates(Convert.ToInt32(this.Employee_id));
            }
            set { }
        }
        public double Premium
        {
            get {
                double expPremium = CountExperiencePremium(this.WorkExperience, this.Position, Convert.ToDouble(this.Base_salary));
                double subPremium;
                switch (this.Position)
                {
                    case "Manager":
                        subPremium = SubordinatesSummSalary * 0.005;
                        break;
                    case "Salesman":
                        subPremium = SubordinatesSummSalary * 0.003;
                        break;
                    default:
                        subPremium = 0;
                        break;
                }

                return expPremium + subPremium;
                }
            set { } 
        }
        public double fullSalary
        {
            get
            {
                return Convert.ToDouble(this.Base_salary) + Convert.ToDouble(this.Premium);
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

        // Запрос к бд
        public static SQLiteDataReader DataTableQuerry(string dataBaseFileName, string querry)
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

        // Возвращает сумму зарплаты подчинённых без учёта премии
        private double getSummSalaryOfSubordinates(int id)
        {
            // Заполнение списка id подчинённых
            List<object> subordinateIdList = FindSubordinates(Convert.ToInt32(id));

            // Переменная для накопления суммы зарплат
            double summSalary = 0;
            
            // Рассчёт суммарной зарплаты подчинённых
            foreach (object subordinateId in subordinateIdList)
            {
                // Находим зарплату, дату найма и должность подчинённого
                SQLiteDataReader reader = DataTableQuerry("base.db", $"SELECT Base_salary, Hire_date, Position FROM Staff WHERE Employee_id = {subordinateId};");
                // Перебираем все записи и рассчитываем суммарную зарплату подчинённых
                while (reader.Read())
                {
                    string position = reader["Position"].ToString(); // Должность подчинённого
                    int experience = getWorkExperience(reader["Hire_date"].ToString()); // Опыт работы подчинённого
                    double baseSubordinateSalary = Convert.ToDouble(reader["Base_salary"]); // Базовая ставка подчинённого
                    
                    // Расчёт премии за стаж подчинённого
                    double experiencePremium = CountExperiencePremium(experience, position, baseSubordinateSalary);
                    // Расчёт премии за подчинённых подчинённого !КРАСИВО ЗАРЕФАКТОРИТЬ ЭТУ ТАБЛИЦУ
                    double subordinatesPremium = CountSubordinatesPremium(getSummSalaryOfSubordinates(Convert.ToInt32(subordinateId)), position, FindSubordinates(Convert.ToInt32(subordinateId)));
                    // Общая премия подчинённого
                    double currPrem = baseSubordinateSalary + experiencePremium + subordinatesPremium;
                    //MessageBox.Show($"baseSubSalary: {baseSubordinateSalary}\nexpPrem: {experiencePremium}\nsubPremium{subordinatesPremium}\ncurrentPremium: {currPrem}");

                    summSalary = summSalary + baseSubordinateSalary + experiencePremium + subordinatesPremium;
                    break;
                }
            }
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
        
        // Расчёт премии сотрудника за стаж
        private double CountExperiencePremium(int experience, string position, double base_salary)
        {
            double premium;
            double coefficient;

            // К премии manager'у и salesman'у надо добавить %суммарной зарплаты всех подчинённых
            switch (position)
            {
                case "Employee":
                    coefficient = experience * 0.03;
                    
                    if (coefficient > 0.3)
                    {
                        premium = base_salary * 0.3;
                    }
                    else
                    {
                        premium = base_salary * coefficient;
                    }

                    break;
                
                case "Manager":
                    coefficient = experience * 0.05;
                    if (coefficient > 0.4)
                    {
                        premium = base_salary * 0.4;
                    }
                    else
                    {
                        premium = base_salary * coefficient;
                    }
                    break;
                
                case "Salesman":
                    coefficient = experience * 0.01;
                    if (coefficient > 0.35)
                    {
                        premium = base_salary * 0.35;
                    }
                    else
                    {
                        premium = base_salary * coefficient;
                    }
                    break;

                default:
                    premium = 1;
                    break;
            }

            return premium;
        }
        
        // Расчёт премии сотрудника за количество подчинённых
        private double CountSubordinatesPremium(double base_salary, string position, List<object> idList)
        {
            double coeff;
            switch (position)
            {
                case "Manager":
                    coeff = 0.005;
                    break;
                case "Salesman":
                    coeff = 0.003;
                    break;
                default:
                    coeff = 0;
                    break;
            }
            double premium = base_salary * idList.Count * coeff;
            return premium;
        }

        // Поиск подчинённых сотрудника
        private List<object> FindSubordinates(int id)
        {
            List<object> idList = new List<object>();
            // Поиск id подчинённых
            SQLiteDataReader reader = DataTableQuerry("base.db", $"SELECT Subordinate_id FROM Bosses WHERE Chief_id = {id};");

            // Список с id подчинённых
            while (reader.Read())
            {
                idList.Add(reader["Subordinate_id"]);
            }
            return idList;
        }
    }
}
