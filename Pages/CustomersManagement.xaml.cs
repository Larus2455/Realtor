using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using System.Windows.Data;

namespace CityNest.Pages
{
    public partial class CustomersManagement : Page
    {
        private Entities _db = new Entities();

        public CustomersManagement()
        {
            InitializeComponent();
            LoadCustomers();
        }

        // Загрузка данных о покупателях
        private void LoadCustomers()
        {
            CustomersGrid.ItemsSource = _db.Customers.ToList();
        }

        // Поиск покупателей
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string query = SearchBox.Text.ToLower();
            var filteredCustomers = _db.Customers
                .Where(c => c.Name.ToLower().Contains(query) ||
                            c.Phone.ToLower().Contains(query) ||
                            c.Status.ToLower().Contains(query))
                .ToList();

            CustomersGrid.ItemsSource = filteredCustomers;
        }

        // Добавление нового покупателя
        private void AddCustomer_Click(object sender, RoutedEventArgs e)
        {
            var customer = new Customers
            {
                Name = "Новый покупатель",
                Phone = "123-456-7890",
                Status = "Potential"
            };

            _db.Customers.Add(customer);
            _db.SaveChanges();
            LoadCustomers(); 
        }

        // Редактирование выбранного покупателя
        private void EditCustomer_Click(object sender, RoutedEventArgs e)
        {
            var selectedCustomer = CustomersGrid.SelectedItem as Customers;
            if (selectedCustomer == null)
            {
                MessageBox.Show("Выберите покупателя для редактирования.");
                return;
            }

            var dialog = new EditCustomerDialog(selectedCustomer);
            if (dialog.ShowDialog() == true)
            {
                _db.SaveChanges();
                LoadCustomers(); 
            }
        }

        // Удаление выбранного покупателя
        private void DeleteCustomer_Click(object sender, RoutedEventArgs e)
        {
            var selectedCustomer = CustomersGrid.SelectedItem as Customers;
            if (selectedCustomer == null)
            {
                MessageBox.Show("Выберите покупателя для удаления.");
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить этого покупателя?", "Подтверждение", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                _db.Customers.Remove(selectedCustomer);
                _db.SaveChanges();
                LoadCustomers(); 
            }
        }

        // Экспорт данных в Excel
        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            var customers = CustomersGrid.ItemsSource as IEnumerable<Customers>;
            if (customers == null || !customers.Any())
            {
                MessageBox.Show("Нет данных для экспорта.");
                return;
            }

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Покупатели");


            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Имя";
            worksheet.Cell(1, 3).Value = "Телефон";
            worksheet.Cell(1, 4).Value = "Статус";

            // Данные
            int row = 2;
            foreach (var customer in customers)
            {
                worksheet.Cell(row, 1).Value = customer.CustomerId;
                worksheet.Cell(row, 2).Value = customer.Name;
                worksheet.Cell(row, 3).Value = customer.Phone;
                worksheet.Cell(row, 4).Value = customer.Status;
                row++;
            }

            // Сохранение файла
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = "Покупатели.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                workbook.SaveAs(saveFileDialog.FileName);
                MessageBox.Show("Данные успешно экспортированы.");
            }
        }

        // Показать сделки выбранного покупателя
        private void ShowDeals_Click(object sender, RoutedEventArgs e)
        {
            var selectedCustomer = CustomersGrid.SelectedItem as Customers;
            if (selectedCustomer == null)
            {
                MessageBox.Show("Выберите покупателя.");
                return;
            }

            var deals = _db.Deals.Where(d => d.CustomerId == selectedCustomer.CustomerId).ToList();
            var dealsWindow = new DealsView(deals);
            dealsWindow.Show();
        }
    }

    // Статические данные для статусов
    public static class CustomerStatuses
    {
        public static List<string> All => new List<string> { "Potential", "Active", "Completed" };
    }

    // Диалоговое окно для редактирования покупателя
    public class EditCustomerDialog : Window
    {
        private Customers _customer;

        public EditCustomerDialog(Customers customer)
        {
            _customer = customer;

            Title = "Редактирование покупателя";
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

            var nameLabel = new Label { Content = "Имя:" };
            var nameTextBox = new TextBox { Text = customer.Name, Margin = new Thickness(0, 0, 0, 10) };
            Grid.SetRow(nameLabel, 0);
            Grid.SetRow(nameTextBox, 1);

            var phoneLabel = new Label { Content = "Телефон:" };
            var phoneTextBox = new TextBox { Text = customer.Phone, Margin = new Thickness(0, 0, 0, 10) };
            Grid.SetRow(phoneLabel, 2);
            Grid.SetRow(phoneTextBox, 3);

            var saveButton = new Button { Content = "Сохранить", Width = 100, Height = 30, Margin = new Thickness(0, 20, 0, 0) };
            Grid.SetRow(saveButton, 4);
            saveButton.Click += (sender, e) =>
            {
                _customer.Name = nameTextBox.Text;
                _customer.Phone = phoneTextBox.Text;
                DialogResult = true;
                Close();
            };

            grid.Children.Add(nameLabel);
            grid.Children.Add(nameTextBox);
            grid.Children.Add(phoneLabel);
            grid.Children.Add(phoneTextBox);
            grid.Children.Add(saveButton);

            Content = grid;
        }
    }

    // Окно просмотра сделок
    public class DealsView : Window
    {
        public DealsView(IEnumerable<Deals> deals)
        {
            Title = "Сделки покупателя";
            Width = 600;
            Height = 400;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var dataGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                Margin = new Thickness(10),
                ItemsSource = deals
            };

            dataGrid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("DealId") });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Дата", Binding = new Binding("DealDate") });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Сумма", Binding = new Binding("Amount") });

            Content = dataGrid;
        }
    }
}