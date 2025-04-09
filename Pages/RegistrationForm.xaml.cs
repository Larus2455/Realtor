using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using ClosedXML.Excel;
using System.Windows.Controls;

namespace CityNest.Pages
{
    public partial class RegistrationForm : Page
    {
        private Entities _db = new Entities();

        public RegistrationForm()
        {
            InitializeComponent();
            LoadUsers(); 
        }

        // Метод для хэширования пароля
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        // Загрузка данных о пользователях
        private void LoadUsers()
        {
            UsersGrid.ItemsSource = _db.Users.ToList();
        }

        // Генерация случайного пароля
        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void GeneratePassword_Click(object sender, RoutedEventArgs e)
        {
            string randomPassword = GenerateRandomPassword();
            PasswordBox.Password = randomPassword;
            ConfirmPasswordBox.Password = randomPassword; 
        }

        // Обработка нажатия на кнопку "Зарегистрировать"
        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;
            var roleItem = RoleComboBox.SelectedItem as ComboBoxItem;
            string role = roleItem?.Content.ToString();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword) || role == null)
            {
                MessageBox.Show("Заполните все поля.");
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают.");
                return;
            }

            if (_db.Users.Any(u => u.Login == login))
            {
                MessageBox.Show("Пользователь с таким логином уже существует.");
                return;
            }

            var newUser = new Users
            {
                Login = login,
                PasswordHash = HashPassword(password),
                Role = role,
                IsLocked = false,
                FailedLoginAttempts = 0
            };

            _db.Users.Add(newUser);
            _db.SaveChanges();

            MessageBox.Show("Пользователь успешно зарегистрирован!");
            ClearFields();
            LoadUsers(); 
        }

        private void ClearFields()
        {
            LoginBox.Clear();
            PasswordBox.Clear();
            ConfirmPasswordBox.Clear();
            RoleComboBox.SelectedIndex = -1;
        }

        private void SearchUsers_Click(object sender, RoutedEventArgs e)
        {
            string query = SearchBox.Text.ToLower();
            var filteredUsers = _db.Users
                .Where(u => u.Login.ToLower().Contains(query) || u.Role.ToLower().Contains(query))
                .ToList();

            UsersGrid.ItemsSource = filteredUsers;
        }

        // Блокировка/разблокировка пользователя
        private void ToggleLock_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var user = button?.DataContext as Users;

            if (user == null)
            {
                MessageBox.Show("Не удалось найти пользователя.");
                return;
            }

            user.IsLocked = !(user.IsLocked ?? false); 

            _db.SaveChanges();
            LoadUsers(); 
        }

        // Экспорт данных о пользователях в Excel
        private void ExportUsersToExcel_Click(object sender, RoutedEventArgs e)
        {
            var users = _db.Users.ToList();
            if (!users.Any())
            {
                MessageBox.Show("Нет данных для экспорта.");
                return;
            }

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Пользователи");

            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Логин";
            worksheet.Cell(1, 3).Value = "Роль";
            worksheet.Cell(1, 4).Value = "Заблокирован";

            int row = 2;
            foreach (var user in users)
            {
                worksheet.Cell(row, 1).Value = user.UserId;
                worksheet.Cell(row, 2).Value = user.Login;
                worksheet.Cell(row, 3).Value = user.Role;
                worksheet.Cell(row, 4).Value = user.IsLocked ?? false ? "Да" : "Нет"; 
                row++;
            }

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = "Пользователи.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                workbook.SaveAs(saveFileDialog.FileName);
                MessageBox.Show("Данные успешно экспортированы.");
            }
        }
    }
}