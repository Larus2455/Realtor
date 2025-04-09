using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;


namespace CityNest.Pages
{
    public partial class ApartmentsManagement : Page
    {
        private Entities _db = new Entities();

        public ApartmentsManagement()
        {
            InitializeComponent();
            LoadApartments();
        }

        // Загрузка данных о квартирах
        private void LoadApartments()
        {
            ApartmentsGrid.ItemsSource = _db.Apartments.ToList();
        }

        // Обработка выбора квартиры в таблице
        private void ApartmentsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedApartment = ApartmentsGrid.SelectedItem as Apartments;
            if (selectedApartment != null && !string.IsNullOrEmpty(selectedApartment.ImagePath))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(selectedApartment.ImagePath, UriKind.RelativeOrAbsolute);
                bitmap.EndInit();
                ApartmentImage.Source = bitmap;
            }
            else
            {
                ApartmentImage.Source = null;
            }
        }

        // Добавление новой квартиры
        private void AddApartment_Click(object sender, RoutedEventArgs e)
        {
            var apartment = new Apartments
            {
                Address = "Новый адрес",
                Rooms = 1,
                Area = 50,
                Price = 100000
            };

            _db.Apartments.Add(apartment);
            _db.SaveChanges();
            LoadApartments(); 
        }

        // Редактирование выбранной квартиры
        private void EditApartment_Click(object sender, RoutedEventArgs e)
        {
            var selectedApartment = ApartmentsGrid.SelectedItem as Apartments;
            if (selectedApartment == null)
            {
                MessageBox.Show("Выберите квартиру для редактирования.");
                return;
            }

            var dialog = new EditApartmentDialog(selectedApartment);
            if (dialog.ShowDialog() == true)
            {
                _db.SaveChanges();
                LoadApartments(); 
            }
        }

        // Удаление выбранной квартиры
        private void DeleteApartment_Click(object sender, RoutedEventArgs e)
        {
            var selectedApartment = ApartmentsGrid.SelectedItem as Apartments;
            if (selectedApartment == null)
            {
                MessageBox.Show("Выберите квартиру для удаления.");
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить эту квартиру?", "Подтверждение", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                _db.Apartments.Remove(selectedApartment);
                _db.SaveChanges();
                LoadApartments(); 
            }
        }

        // Добавление изображения для выбранной квартиры
        private void AddImage_Click(object sender, RoutedEventArgs e)
        {
            var selectedApartment = ApartmentsGrid.SelectedItem as Apartments;
            if (selectedApartment == null)
            {
                MessageBox.Show("Выберите квартиру для добавления изображения.");
                return;
            }

            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Изображения (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                selectedApartment.ImagePath = filePath; 
                _db.SaveChanges();
                LoadApartments(); 

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
                bitmap.EndInit();
                ApartmentImage.Source = bitmap;
            }
        }
    }

    // Диалоговое окно для редактирования квартиры
    public class EditApartmentDialog : Window
    {
        private Apartments _apartment;

        public EditApartmentDialog(Apartments apartment)
        {
            _apartment = apartment;

            Title = "Редактирование квартиры";
            Width = 400;
            Height = 300;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var grid = new Grid();
            grid.Margin = new Thickness(20);
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var addressLabel = new Label { Content = "Адрес:" };
            var addressTextBox = new TextBox { Text = apartment.Address, Margin = new Thickness(0, 0, 0, 10) };
            Grid.SetRow(addressLabel, 0);
            Grid.SetRow(addressTextBox, 1);

            var roomsLabel = new Label { Content = "Количество комнат:" };
            var roomsTextBox = new TextBox { Text = apartment.Rooms.ToString(), Margin = new Thickness(0, 0, 0, 10) };
            Grid.SetRow(roomsLabel, 2);
            Grid.SetRow(roomsTextBox, 3);

            var saveButton = new Button { Content = "Сохранить", Width = 100, Height = 30, Margin = new Thickness(0, 20, 0, 0) };
            Grid.SetRow(saveButton, 4);
            saveButton.Click += (sender, e) =>
            {
                apartment.Address = addressTextBox.Text;
                apartment.Rooms = int.Parse(roomsTextBox.Text);
                DialogResult = true;
                Close();
            };

            grid.Children.Add(addressLabel);
            grid.Children.Add(addressTextBox);
            grid.Children.Add(roomsLabel);
            grid.Children.Add(roomsTextBox);
            grid.Children.Add(saveButton);

            Content = grid;
        }
    }
}