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
        public MainWindow()
        {
            InitializeComponent();
            InitializeDataBase();
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
        private void ButtonClick_Auth(object sender, RoutedEventArgs e)
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
                        AdminWindow();
                    }
                    else
                    {
                        UserWindow();
                    }
                    

                }
                else
                {
                    MessageBox.Show("Неверный пароль");
                }
            }
        }

        private void AdminWindow()
        {
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


            Button showSumSalary_Button = new Button
            {
                Content = "Показать суммарную зарплату",
                Width = width,
                Margin = thickness
            };

            Button back_Button= new Button();
            back_Button.Content = "Назад";
            back_Button.Width = 286;
            back_Button.Margin = thickness;


            formAuth.Children.Add(showAccBase_Button);
            formAuth.Children.Add(showStaffBase_Button);
            formAuth.Children.Add(showBossesBase_Button);
            formAuth.Children.Add(addEmployee_Button);
            formAuth.Children.Add(showSumSalary_Button);
            formAuth.Children.Add(back_Button);
        }

        // Все три метода можно объединить в один, но у меня не получилось настроить передачу аргументов, например, с названием таблицы, при нажатии на кноку показа базы
        // TODO
        // 1. Дополнить формулу расчёта премии, учитывая кол-во подчинённых сотрудников  (ХЗ КАК)
        private void ShowBaseAccounts(object sender, RoutedEventArgs e)
        {
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

            // Делаем кнопку "НАЗАД"
            Button buttonBack = new Button
            {
                Content = "Назад",
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom
            };
            //buttonBack.Click += AdminWindow;
            grid.Children.Add(buttonBack);

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

        private void ShowBaseStaff(object sender, RoutedEventArgs e)
        {
            // Делаем кнопку "НАЗАД"
            Button buttonBack = new Button
            {
                Content = "Назад",
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom
            };
            //buttonBack.Click += AdminWindow;
            grid.Children.Add(buttonBack);
            
            formBorder.Width = 600;
            formAuth.Width = 600;
            // Очищаем меню
            formAuth.Children.Clear();
            // Таблица с данными
            DataGrid dataGrid = new DataGrid
            {
                Width = 590,
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

                    //Employee test = new Employee(name, hire_date, position, base_salary, login);
                    //MessageBox.Show(test.CountPremium().ToString());

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
                // Заполняем datagrid записями
                dataGrid.ItemsSource = staffList;
            }

            else
                MessageBox.Show("Database is empty");
        }

        private void ShowBaseBosses(object sender, RoutedEventArgs e)
        {
            // Делаем кнопку "НАЗАД"
            Button buttonBack = new Button
            {
                Content = "Назад",
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom
            };
            //buttonBack.Click += AdminWindow;
            grid.Children.Add(buttonBack);

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



        private void UserWindow()
        {
            MessageBox.Show($"Hello, {textBox_Login.Text}");
            // Расположение кнопок для пользователя




        }
    }
}

