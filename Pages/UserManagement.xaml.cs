using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using ClosedXML.Excel;
using System.Windows.Controls;

namespace CityNest.Pages
{
    public partial class UserManagement : Page
    {
        private Entities _db = new Entities();

        public UserManagement()
        {
            InitializeComponent();
            LoadUsers();
            UpdateStatistics();
        }

        private void LoadUsers()
        {
            UsersGrid.ItemsSource = _db.Users.ToList();
        }

        // Обновление статистики пользователей
        private void UpdateStatistics()
        {
            int adminCount = _db.Users.Count(u => u.Role == "Admin");
            int realtorCount = _db.Users.Count(u => u.Role == "Realtor");

            AdminCountText.Text = adminCount.ToString();
            RealtorCountText.Text = realtorCount.ToString();
        }

        // Поиск пользователей
        private void SearchUsers_Click(object sender, RoutedEventArgs e)
        {
            string query = SearchBox.Text.ToLower();
            var filteredUsers = _db.Users
                .Where(u => u.Login.ToLower().Contains(query) || u.Role.ToLower().Contains(query))
                .ToList();

            ApplyStatusFilter(filteredUsers);
        }

        // Фильтрация по статусу
        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var users = _db.Users.ToList();
            ApplyStatusFilter(users);
        }

        private void ApplyStatusFilter(IEnumerable<Users> users)
        {
            var selectedItem = StatusFilter.SelectedItem as ComboBoxItem;
            if (selectedItem == null) return;

            string status = selectedItem.Content.ToString();
            IEnumerable<Users> filteredUsers = users;

            if (status == "Активные")
            {
                filteredUsers = users.Where(u => u.IsLocked == false); 
            }
            else if (status == "Заблокированные")
            {
                filteredUsers = users.Where(u => u.IsLocked == true); 
            }

            UsersGrid.ItemsSource = filteredUsers;
        }

        // Очистка фильтров
        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Clear();
            StatusFilter.SelectedIndex = 0;
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
                worksheet.Cell(row, 4).Value = user.IsLocked == true ? "Да" : "Нет"; 
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


        private void RefreshData_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
            UpdateStatistics();
            MessageBox.Show("Данные успешно обновлены.");
        }

        // Сброс пароля пользователя
        private void ResetPassword_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var user = button?.DataContext as Users;

            if (user == null)
            {
                MessageBox.Show("Не удалось найти пользователя.");
                return;
            }

            string newPassword = GenerateRandomPassword();
            user.PasswordHash = HashPassword(newPassword);
            _db.SaveChanges();

            LogAction($"Сброс пароля для пользователя {user.Login}");
            MessageBox.Show($"Новый пароль для пользователя {user.Login}: {newPassword}");
        }

        // Отправка email
        private void SendEmail_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var user = button?.DataContext as Users;

            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                MessageBox.Show("Не удалось найти пользователя или его email.");
                return;
            }

            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("your-email@gmail.com", "your-password"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("your-email@gmail.com"),
                    Subject = "Уведомление от CityNest",
                    Body = $"Здравствуйте, {user.Login}!\n\nВаш аккаунт был изменен администратором.",
                    IsBodyHtml = false,
                };
                mailMessage.To.Add(user.Email);

                smtpClient.Send(mailMessage);
                LogAction($"Отправлен email пользователю {user.Login}");
                MessageBox.Show($"Email успешно отправлен пользователю {user.Login}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке email: {ex.Message}");
            }
        }

        // Удаление пользователя
        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var user = button?.DataContext as Users;

            if (user == null)
            {
                MessageBox.Show("Не удалось найти пользователя.");
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить пользователя {user.Login}?", "Подтверждение", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                _db.Users.Remove(user);
                _db.SaveChanges();
                LoadUsers();
                UpdateStatistics();
                LogAction($"Удален пользователь {user.Login}");
                MessageBox.Show("Пользователь успешно удален.");
            }
        }

        // Генерация случайного пароля
        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // Хэширование пароля
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        // Логирование действий администратора
        private void LogAction(string action)
        {
            var logMessage = $"{DateTime.Now}: {action}";
            File.AppendAllText("admin_actions.log", logMessage + Environment.NewLine);
        }
    }

    // Статические данные для ролей
    public static class UserRole
    {
        public static List<string> All => new List<string> { "Admin", "Realtor" };
    }
   
}