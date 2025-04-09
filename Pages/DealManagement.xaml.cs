using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using System.Data.Entity;
using System.Drawing;
using System.Windows.Data;

namespace CityNest.Pages
{
    public partial class DealManagement : Page
    {
        private Entities _db = new Entities();

        public DealManagement()
        {
            InitializeComponent();
            LoadDeals();
        }

        // Загрузка данных о сделках
        private void LoadDeals()
        {
            DealsGrid.ItemsSource = _db.Deals
                .Include("Customers")
                .Include("Apartments")
                .ToList();
            UpdateTotalAmount(); 
        }

        // Поиск сделок
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string query = SearchBox.Text.ToLower();
            var filteredDeals = _db.Deals
                .Include("Customers")
                .Include("Apartments")
                .Where(d => d.Customers.Name.ToLower().Contains(query) ||
                            d.Apartments.Address.ToLower().Contains(query) ||
                            d.Amount.ToString().Contains(query))
                .ToList();

            DealsGrid.ItemsSource = filteredDeals;
        }

        // Добавление новой сделки
        private void AddDeal_Click(object sender, RoutedEventArgs e)
        {
            var customers = _db.Customers.ToList();
            var apartments = _db.Apartments.ToList();

            if (!customers.Any() || !apartments.Any())
            {
                MessageBox.Show("Нет доступных покупателей или квартир для оформления сделки.");
                return;
            }

            var dialog = new AddDealDialog(customers, apartments);
            if (dialog.ShowDialog() == true)
            {
                var newDeal = new Deals
                {
                    CustomerId = dialog.SelectedCustomer.CustomerId,
                    ApartmentId = dialog.SelectedApartment.ApartmentId,
                    DealDate = DateTime.Now,
                    Amount = dialog.Amount
                };

                _db.Deals.Add(newDeal);
                _db.SaveChanges();
                LoadDeals(); 
            }
        }

        // Редактирование выбранной сделки
        private void EditDeal_Click(object sender, RoutedEventArgs e)
        {
            var selectedDeal = DealsGrid.SelectedItem as Deals;
            if (selectedDeal == null)
            {
                MessageBox.Show("Выберите сделку для редактирования.");
                return;
            }

            var customers = _db.Customers.ToList();
            var apartments = _db.Apartments.ToList();

            var dialog = new AddDealDialog(customers, apartments, selectedDeal);
            if (dialog.ShowDialog() == true)
            {
                selectedDeal.CustomerId = dialog.SelectedCustomer.CustomerId;
                selectedDeal.ApartmentId = dialog.SelectedApartment.ApartmentId;
                selectedDeal.Amount = dialog.Amount;

                _db.SaveChanges();
                LoadDeals(); 
            }
        }

        // Удаление выбранной сделки
        private void DeleteDeal_Click(object sender, RoutedEventArgs e)
        {
            var selectedDeal = DealsGrid.SelectedItem as Deals;
            if (selectedDeal == null)
            {
                MessageBox.Show("Выберите сделку для удаления.");
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить эту сделку?", "Подтверждение", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                _db.Deals.Remove(selectedDeal);
                _db.SaveChanges();
                LoadDeals(); 
            }
        }

        // Экспорт данных в Excel
        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            var deals = DealsGrid.ItemsSource as IEnumerable<Deals>;
            if (deals == null || !deals.Any())
            {
                MessageBox.Show("Нет данных для экспорта.");
                return;
            }

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Сделки");


            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Покупатель";
            worksheet.Cell(1, 3).Value = "Квартира";
            worksheet.Cell(1, 4).Value = "Дата";
            worksheet.Cell(1, 5).Value = "Сумма";

            // Данные
            int row = 2;
            foreach (var deal in deals)
            {
                worksheet.Cell(row, 1).Value = deal.DealId;
                worksheet.Cell(row, 2).Value = deal.Customers.Name;
                worksheet.Cell(row, 3).Value = deal.Apartments.Address;
                worksheet.Cell(row, 4).Value = deal.DealDate.ToString("dd.MM.yyyy");
                worksheet.Cell(row, 5).Value = deal.Amount;
                row++;
            }


            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = "Сделки.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                workbook.SaveAs(saveFileDialog.FileName);
                MessageBox.Show("Данные успешно экспортированы.");
            }
        }

        // Показать историю сделок для конкретного покупателя
        private void ShowCustomerHistory_Click(object sender, RoutedEventArgs e)
        {
            var selectedDeal = DealsGrid.SelectedItem as Deals;
            if (selectedDeal == null)
            {
                MessageBox.Show("Выберите сделку для просмотра истории.");
                return;
            }

            var customerHistory = _db.Deals
                .Where(d => d.CustomerId == selectedDeal.CustomerId)
                .Include("Apartments")
                .ToList();

            var historyWindow = new CustomerHistoryView(customerHistory);
            historyWindow.Show();
        }

        private void UpdateTotalAmount()
        {
            decimal totalAmount = _db.Deals.Sum(d => (decimal?)d.Amount) ?? 0;
            TotalAmountText.Text = $"Общая сумма сделок: {totalAmount} руб.";
        }
    }

    // Диалоговое окно для добавления/редактирования сделки
    public class AddDealDialog : Window
    {
        public Customers SelectedCustomer { get; private set; }
        public Apartments SelectedApartment { get; private set; }
        public decimal Amount { get; private set; }

        public AddDealDialog(IEnumerable<Customers> customers, IEnumerable<Apartments> apartments, Deals deal = null)
        {
            Title = deal == null ? "Добавление сделки" : "Редактирование сделки";
            Width = 400;
            Height = 350;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var grid = new Grid();
            grid.Margin = new Thickness(20);
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var customerLabel = new Label { Content = "Покупатель:" };
            var customerComboBox = new ComboBox { ItemsSource = customers, DisplayMemberPath = "Name", Margin = new Thickness(0, 0, 0, 10) };
            Grid.SetRow(customerLabel, 0);
            Grid.SetRow(customerComboBox, 1);

            var apartmentLabel = new Label { Content = "Квартира:" };
            var apartmentComboBox = new ComboBox { ItemsSource = apartments, DisplayMemberPath = "Address", Margin = new Thickness(0, 0, 0, 10) };
            Grid.SetRow(apartmentLabel, 2);
            Grid.SetRow(apartmentComboBox, 3);

            var amountLabel = new Label { Content = "Сумма:" };
            var amountTextBox = new TextBox { Margin = new Thickness(0, 0, 0, 10) };
            Grid.SetRow(amountLabel, 4);
            Grid.SetRow(amountTextBox, 5);

            var saveButton = new Button { Content = "Сохранить", Width = 100, Height = 30, Margin = new Thickness(0, 20, 0, 0) };
            Grid.SetRow(saveButton, 6);
            saveButton.Click += (sender, e) =>
            {
                SelectedCustomer = customerComboBox.SelectedItem as Customers;
                SelectedApartment = apartmentComboBox.SelectedItem as Apartments;
                Amount = decimal.Parse(amountTextBox.Text);

                if (SelectedCustomer == null || SelectedApartment == null || Amount <= 0)
                {
                    MessageBox.Show("Заполните все поля корректно.");
                    return;
                }

                DialogResult = true;
                Close();
            };

            if (deal != null)
            {
                customerComboBox.SelectedItem = customers.FirstOrDefault(c => c.CustomerId == deal.CustomerId);
                apartmentComboBox.SelectedItem = apartments.FirstOrDefault(a => a.ApartmentId == deal.ApartmentId);
                amountTextBox.Text = deal.Amount.ToString();
            }

            grid.Children.Add(customerLabel);
            grid.Children.Add(customerComboBox);
            grid.Children.Add(apartmentLabel);
            grid.Children.Add(apartmentComboBox);
            grid.Children.Add(amountLabel);
            grid.Children.Add(amountTextBox);
            grid.Children.Add(saveButton);

            Content = grid;
        }
    }

    // Окно для просмотра истории сделок
    public class CustomerHistoryView : Window
    {
        public CustomerHistoryView(IEnumerable<Deals> deals)
        {
            Title = "История сделок покупателя";
            Width = 600;
            Height = 400;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var dataGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                Margin = new Thickness(10),
                ItemsSource = deals
            };

            // Добавление столбцов
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("DealId") });

            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Квартира",
                Binding = new Binding("Apartments.Address")
            });

            // Исправленный столбец "Дата"
            var dateColumn = new DataGridTextColumn
            {
                Header = "Дата",
                Binding = new Binding("DealDate") { StringFormat = "{0:dd.MM.yyyy}" }
            };
            dataGrid.Columns.Add(dateColumn);

            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Сумма", Binding = new Binding("Amount") });

            Content = dataGrid;
        }
    }
}