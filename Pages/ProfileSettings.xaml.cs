using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.IO;
using System.Windows.Shapes;

namespace CityNest.Pages
{
    public partial class ProfileSettings : Page
    {
        private Entities _db = new Entities();
        private Customers _currentCustomer;

        public ProfileSettings()
        {
            InitializeComponent();
            LoadCustomerData();
        }

        private void LoadCustomerData()
        {
            int currentUserId = 1; 
            _currentCustomer = _db.Customers.FirstOrDefault(c => c.CustomerId == currentUserId);

            if (_currentCustomer != null)
            {
                NameTextBox.Text = _currentCustomer.Name;
                StatusTextBox.Text = _currentCustomer.Status;
                PhoneTextBox.Text = _currentCustomer.Phone;

                if (!string.IsNullOrEmpty(_currentCustomer.AvatarPath) && File.Exists(_currentCustomer.AvatarPath))
                {
                    AvatarImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(_currentCustomer.AvatarPath));
                }
            }
        }

        private void UploadAvatar_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Изображения (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
                Title = "Выберите изображение для аватара"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                _currentCustomer.AvatarPath = filePath; 

                AvatarImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(filePath));
            }
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            if (_currentCustomer == null)
            {
                MessageBox.Show("Ошибка загрузки данных пользователя.");
                return;
            }

            _currentCustomer.Name = NameTextBox.Text;
            _currentCustomer.Status = StatusTextBox.Text;
            _currentCustomer.Phone = PhoneTextBox.Text;

            _db.SaveChanges();

            MessageBox.Show("Изменения успешно сохранены.");
        }
    }
}