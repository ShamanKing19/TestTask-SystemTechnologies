using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using StaffClass;
using AccountClass;
using BossesClass;
using System.Windows.Controls.Primitives;

namespace TestTask
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string dbFileName;
        private SQLiteConnection dbConnection;
        private SQLiteCommand sqlCommand;
        private Button backButton = new Button 
        {
            Content = "Назад",
            Width = 286,
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 5)
        };
        private Button backToAuthButton = new Button
        {
            Content = "Назад",
            Width = 286,
            Margin = new Thickness(0, 5, 0, 10),
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        private TextBlock sumSalaryTextBlock = new TextBlock
        {
            Height = 20,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 10, 0, 0)
        };

        public MainWindow()
        {
            InitializeComponent();
            InitializeDataBase();
            backButton.Click += AdminWindow;
            backToAuthButton.Click += BackToAuthWindow;
            decimal counter = 0;
        }

        public void InitializeDataBase()
        {
            // Переменные для работы с БД
            dbConnection = new SQLiteConnection();
            sqlCommand = new SQLiteCommand();
            dbFileName = "base.db";
            bool newBaseFlag = false;

            // Создание файла с базой, если его не существует
            if (!File.Exists(dbFileName))
            {
                SQLiteConnection.CreateFile(dbFileName);
                newBaseFlag = true;
            }

            try
            {
                dbConnection = new SQLiteConnection($"Data Source= {dbFileName};Version=3;");
                dbConnection.Open();
                sqlCommand.Connection = dbConnection;

                // Создание таблицы Staff
                sqlCommand.CommandText = "CREATE TABLE IF NOT EXISTS Staff (Employee_id INTEGER NOT NULL UNIQUE, Name TEXT NOT NULL, Hire_date TEXT, Position TEXT NOT NULL, Base_salary INTEGER NOT NULL, Login TEXT NOT NULL UNIQUE, Premium TEXT, PRIMARY KEY(Employee_id AUTOINCREMENT), FOREIGN KEY(Login) REFERENCES Accounts(Login))";
                sqlCommand.ExecuteNonQuery();

                // Создание таблицы Accounts
                sqlCommand.CommandText = "CREATE TABLE IF NOT EXISTS Accounts (Login TEXT NOT NULL UNIQUE, Password TEXT NOT NULL, PRIMARY KEY(Login))";
                sqlCommand.ExecuteNonQuery();

                // Внесение данных суперпользователя, при условии, что таблица ещё не существовала
                sqlCommand.CommandText = newBaseFlag == true ? "INSERT INTO Accounts (Login, Password) values (\"admin\", \"admin\")" : "";
                sqlCommand.ExecuteNonQuery();


                // Создание таблицы Bosses
                sqlCommand.CommandText = "CREATE TABLE IF NOT EXISTS Bosses (Chief_id INTEGER NOT NULL, Subordinate_id INTEGER NOT NULL, FOREIGN KEY(Chief_id) REFERENCES Staff (Employee_id), FOREIGN KEY(Subordinate_id) REFERENCES Staff (Employee_id))";
                sqlCommand.ExecuteNonQuery();

                // Вывод, если подключено успешно
                //Output.Text = "Connected";
            }
            catch (SQLiteException ex)
            {
                //Output.Text = "Disconnected";
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // Авторизация пользователя
        private void buttonAuth_Click(object sender, RoutedEventArgs e)
        {
            // Переменная с базой данных
            DataTable dTable = new DataTable();

            // Данные пользователя
            string login = textBox_Login.Text;
            string password = passwordBox_Password.Password;

            // Выполняем запрос на проверку пароля
            string sqlQuery = $"SELECT Password FROM Accounts WHERE Login = \"{login}\"";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqlQuery, dbConnection);
            adapter.Fill(dTable);

            // Проверка на наличие аккаунта с таким логином
            if (dTable.Rows.Count == 0)
            {
                MessageBox.Show("Нет аккаунтов с таким логином");
            }
            else
            {
                // Пароль от аккаунта из базы данных
                string chosenAccPassword = dTable.Rows[0]["Password"].ToString();

                // Проверка пароля от аккаунта
                if (password == chosenAccPassword)
                {
                    // Удаляем элементы окна авторизации
                    formAuth.Children.Clear();
                    
                    //MessageBox.Show("Вы авторизовались!");
                    // Расположение элементов в зависимости от типа пользователя
                    if (login == "admin") 
                    {
                        AdminWindow(sender, e);
                    }
                    else
                    {
                        UserWindow(sender, e);
                    }
                    

                }
                else
                {
                    MessageBox.Show("Неверный пароль");
                }
            }
        }

        private void AdminWindow(object sender, RoutedEventArgs e)
        {
            grid.Children.Remove(sumSalaryTextBlock); // Удаление блока с суммарной зарплатой
            grid.Children.Remove(backButton); // Убираем кнопку "Назад"
            formAuth.Children.Clear(); // Очистка контейнера

            // Параметры кнопок
            const int margin = 5;
            const int width = 286;
            Thickness thickness = new Thickness(0, margin, 0, margin);

            // Расположение и настройка кнопок для админа
            Button showAccBase_Button = new Button
            {
                Content = "Показать базу с аккаунтами",
                Width = width,
                Margin = thickness
            };
            showAccBase_Button.Click += ShowBaseAccounts;

            Button showStaffBase_Button = new Button
            {
                Content = "Показать базу с сотрудниками",
                Width = width,
                Margin = thickness
            };
            showStaffBase_Button.Click += ShowBaseStaff;

            Button showBossesBase_Button = new Button
            {
                Content = "Показать базу с начальниками",
                Width = width,
                Margin = thickness
            };
            showBossesBase_Button.Click += ShowBaseBosses;

            Button addEmployee_Button = new Button
            {
                Content = "Добавить сотрудника",
                Width = width,
                Margin = thickness
            };
            addEmployee_Button.Click += AddEmployee;

            formAuth.Children.Add(showAccBase_Button);
            formAuth.Children.Add(showStaffBase_Button);
            formAuth.Children.Add(showBossesBase_Button);
            formAuth.Children.Add(addEmployee_Button);
            formAuth.Children.Add(backToAuthButton);
        }

        // Окно пользователя с информацией о зарплате
        private void UserWindow(object sender, RoutedEventArgs e)
        {
            //// Расположение кнопок для пользователя
            // Настройка границ контейнера с таблицей
            formBorder.Width = 200;
            formAuth.Width = 200;

            // Поле с зарплатой сотрудника
            TextBlock salaryTextBlock = new TextBlock
            {
                Height = 20,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            grid.Children.Add(salaryTextBlock);
            grid.Children.Add(backToAuthButton);

            // Подключение к БД
            SQLiteConnection dbConnection = new SQLiteConnection($"Data Source = base.db;Version=3;");
            SQLiteCommand sqlCommand = new SQLiteCommand();
            dbConnection.Open();
            sqlCommand.Connection = dbConnection;
            sqlCommand.CommandText = $"SELECT Base_salary, Position, Hire_date FROM Staff WHERE Login = \"{textBox_Login.Text}\";";
            SQLiteDataReader reader = sqlCommand.ExecuteReader();

            // Создание экземпляра класса для авторизовавшегося пользователя
            Employee user = new Employee();

            // Без цикла не работает
            while (reader.Read())
            {
                user.Base_salary = reader["Base_salary"].ToString();
                user.Position = reader["Position"].ToString();
                user.Hire_date = reader["Hire_date"].ToString();
            }

            // Вывод зарплаты в TextBox
            salaryTextBlock.Text = $"Ваша зарплата: {Convert.ToDouble(user.Base_salary) + Convert.ToDouble(user.Premium)}";
        }


        /* Все три метода с выводом таблиц можно объединить в один,
         но у меня не получилось настроить передачу аргументов,           
        например, с названием таблицы, при нажатии на кноку показа базы  */
        
        // Отображает таблицу "Accounts" с аккаунтами работников 
        private void ShowBaseAccounts(object sender, RoutedEventArgs e)
        {
            // Настройка границ контейнера с таблицей
            formBorder.Width = 400;
            formAuth.Width = 400;

            // Очищаем меню
            formAuth.Children.Clear();
            // Таблица с данными
            DataGrid dataGrid = new DataGrid
            {
                Width = 390,
                Height = 290,
            };


            // Размещаем таблицу
            formAuth.Children.Add(dataGrid);

            // Размещаем кнопку "НАЗАД"
            grid.Children.Add(backButton);

            // Заполняем таблицу
            DataTable dTable = new DataTable();
            string sqlQuery = "SELECT * FROM Accounts";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqlQuery, dbConnection);
            adapter.Fill(dTable);


            // Узнаём количество записей
            int rows = dTable.Rows.Count;
            if (rows > 0)
            {
                List<Account> accountList = new List<Account>();
                for (int i = 0; i < rows; i++)
                {
                    
                    string log = dTable.Rows[i].ItemArray[0].ToString();
                    string pass = dTable.Rows[i].ItemArray[1].ToString();

                    // Так не добавляется
                    //accountList.Add(new Account(log, pass));
                    
                    // А так добавляется, выяснить почему
                    accountList.Add(new Account
                    {
                        login = log,
                        password = pass
                    });

                }

                dataGrid.ItemsSource = accountList;
            }

            else
                MessageBox.Show("Database is empty");
        }

        // Отображает таблицу "Staff" с сотрудниками, а также столбец с премией и зарплатой
        private void ShowBaseStaff(object sender, RoutedEventArgs e)
        {
            // Настройка границ контейнера с таблицей
            formBorder.Width = 750;
            formAuth.Width = 750;

            // Размещаем кнопку "НАЗАД"
            grid.Children.Add(backButton);

            // Поле с общей зарплатой
            grid.Children.Add(sumSalaryTextBlock);

            // Очищаем меню
            formAuth.Children.Clear();
            
            // Таблица с данными
            DataGrid dataGrid = new DataGrid
            {
                Width = 740,
                Height = 290,
            };
            
            // Размещаем таблицу
            formAuth.Children.Add(dataGrid);

            // Заполняем таблицу
            DataTable dTable = new DataTable();
            string sqlQuery = "SELECT * FROM Staff";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqlQuery, dbConnection);
            adapter.Fill(dTable);

            // Узнаём количество записей
            int rows = dTable.Rows.Count;
            if (rows > 0)
            {
                // Список для будущих записей
                List<Employee> staffList = new List<Employee>();
                for (int i = 0; i < rows; i++)
                {
                    string id = dTable.Rows[i].ItemArray[0].ToString();
                    string name = dTable.Rows[i].ItemArray[1].ToString();
                    string hire_date = dTable.Rows[i].ItemArray[2].ToString();
                    string position = dTable.Rows[i].ItemArray[3].ToString();
                    string base_salary = dTable.Rows[i].ItemArray[4].ToString();
                    string login = dTable.Rows[i].ItemArray[5].ToString();


                    staffList.Add(new Employee
                    {
                        Employee_id = id,
                        Name = name,
                        Hire_date = hire_date,
                        Position = position,
                        Base_salary = base_salary,
                        Login = login
                    });

                }

                double summSalary = 0;
                foreach (Employee employee in staffList)
                {
                    summSalary += employee.fullSalary;

                }

                // Вывод суммарной зарплаты
                sumSalaryTextBlock.Text = $"Суммарная зарплата к выдаче: {summSalary}";
                // Заполняем datagrid записями
                dataGrid.ItemsSource = staffList;
            }

            else
                MessageBox.Show("Database is empty");
        }

        // Отображает таблицу "Bosses" с начальниками и подчинёнными
        private void ShowBaseBosses(object sender, RoutedEventArgs e)
        {
            // Настройка границ контейнера с таблицей
            formBorder.Width = 400;
            formAuth.Width = 400;

            // Делаем кнопку "НАЗАД"
            grid.Children.Add(backButton);

            // Очищаем меню
            formAuth.Children.Clear();
            // Таблица с данными
            DataGrid dataGrid = new DataGrid
            {
                Width = 390,
                Height = 290,
            };
            // Размещаем таблицу
            formAuth.Children.Add(dataGrid);


            // Заполняем таблицу
            DataTable dTable = new DataTable();
            string sqlQuery = "SELECT * FROM Bosses";
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqlQuery, dbConnection);
            adapter.Fill(dTable);

            // Узнаём количество записей
            int rows = dTable.Rows.Count;
            if (rows > 0)
            {
                // Список для будущих записей
                List<ManageLink> bossesList = new List<ManageLink>();
                for (int i = 0; i < rows; i++)
                {
                    string chief = dTable.Rows[i].ItemArray[0].ToString();
                    string subord = dTable.Rows[i].ItemArray[1].ToString();

                    bossesList.Add(new ManageLink
                    {
                        Chief_id = chief,
                        Subordinate_id = subord
                    });

                }
                // Заполняем datagrid записями
                dataGrid.ItemsSource = bossesList;
            }

            else
                MessageBox.Show("Database is empty");
        }

        // Добавление сотрудника и его аккаунта с паролем
        private void AddEmployee(object sender, RoutedEventArgs e)
        {

        }

        // Возврат к окну авторизации
        private void BackToAuthWindow(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }


    }
}

