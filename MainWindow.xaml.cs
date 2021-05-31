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

        public void InitializeDataBase()
        {
            // Создание файла с базой, если его не существует
            if (!File.Exists("base.db")) 
            {
                SQLiteConnection.CreateFile("base.db");
            }

            // Переменные для работы с БД
            dbConnection = new SQLiteConnection();
            sqlCommand = new SQLiteCommand();

            dbFileName = "base.db";
            //Output.Text = "Disconnected";

            // Создаём файл с бд  если его нет
            if (!File.Exists(dbFileName))
                SQLiteConnection.CreateFile(dbFileName);

            try
            {
                dbConnection = new SQLiteConnection($"Data Source= {dbFileName};Version=3;");
                dbConnection.Open();
                sqlCommand.Connection = dbConnection;

                // Создание таблицы Staff
                sqlCommand.CommandText = "CREATE TABLE IF NOT EXISTS Staff (Employee_id INTEGER NOT NULL UNIQUE, Name TEXT NOT NULL, Hire_date TEXT, Position TEXT NOT NULL, Base_salary INTEGER NOT NULL, Login TEXT NOT NULL UNIQUE, PRIMARY KEY(Employee_id AUTOINCREMENT), FOREIGN KEY(Login) REFERENCES Accounts(Login))";
                sqlCommand.ExecuteNonQuery();

                // Создание таблицы Accounts
                sqlCommand.CommandText = "CREATE TABLE IF NOT EXISTS Accounts (Login TEXT NOT NULL UNIQUE, Password TEXT NOT NULL, PRIMARY KEY(Login))";
                sqlCommand.ExecuteNonQuery();
                
                // Внесение данных суперпользователя
                sqlCommand.CommandText = "INSERT INTO Accounts (Login, Password) values (\"admin\", \"admin\")";
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

        public MainWindow()
        {
            InitializeComponent();
            InitializeDataBase();
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
                    MessageBox.Show("Вы авторизовались!");
                }
                else
                {
                    MessageBox.Show("Неверный пароль");
                }
            }
        }
    }
}
