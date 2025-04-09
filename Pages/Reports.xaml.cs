using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Windows.Data;

namespace CityNest.Pages
{
    public partial class Reports : Page
    {
        private Entities _db = new Entities();

        public Reports()
        {
            InitializeComponent();
        }

        // Формирование отчета
        private void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedReport = ReportTypeComboBox.SelectedItem as ComboBoxItem;
            if (selectedReport == null)
            {
                MessageBox.Show("Выберите тип отчета.");
                return;
            }

            var startDate = StartDatePicker.SelectedDate;
            var endDate = EndDatePicker.SelectedDate;

            if (!startDate.HasValue || !endDate.HasValue)
            {
                MessageBox.Show("Выберите период.");
                return;
            }

            switch (selectedReport.Content.ToString())
            {
                case "Доходы и расходы за период":
                    GenerateIncomeExpensesReport(startDate.Value, endDate.Value);
                    break;

                case "Расходы по категориям":
                    GenerateCategoryExpensesReport(startDate.Value, endDate.Value);
                    break;

                default:
                    MessageBox.Show("Неизвестный тип отчета.");
                    break;
            }
        }

        // Формирование отчета "Доходы и расходы за период"
        private void GenerateIncomeExpensesReport(DateTime startDate, DateTime endDate)
        {
            var deals = _db.Deals
                .Where(d => d.DealDate >= startDate && d.DealDate <= endDate)
                .ToList();

            var reportData = deals
                .GroupBy(d => d.DealDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalIncome = g.Sum(d => d.Amount)
                })
                .OrderBy(r => r.Date)
                .ToList();

            ReportDataGrid.Columns.Clear();
            ReportDataGrid.Columns.Add(new DataGridTextColumn { Header = "Дата", Binding = new Binding("Date") { StringFormat = "{0:dd.MM.yyyy}" } });
            ReportDataGrid.Columns.Add(new DataGridTextColumn { Header = "Общий доход", Binding = new Binding("TotalIncome") });

            ReportDataGrid.ItemsSource = reportData;
        }

        // Формирование отчета "Расходы по категориям"
        private void GenerateCategoryExpensesReport(DateTime startDate, DateTime endDate)
        {
            var apartments = _db.Apartments.ToList();
            var deals = _db.Deals
                .Where(d => d.DealDate >= startDate && d.DealDate <= endDate)
                .ToList();

            var reportData = deals
                .GroupBy(d => d.Apartments.Address)
                .Select(g => new
                {
                    Category = g.Key,
                    TotalExpenses = g.Sum(d => d.Amount)
                })
                .OrderByDescending(r => r.TotalExpenses)
                .ToList();

            ReportDataGrid.Columns.Clear();
            ReportDataGrid.Columns.Add(new DataGridTextColumn { Header = "Категория (Адрес)", Binding = new Binding("Category") });
            ReportDataGrid.Columns.Add(new DataGridTextColumn { Header = "Общие расходы", Binding = new Binding("TotalExpenses") });

            ReportDataGrid.ItemsSource = reportData;
        }

        // Экспорт в Excel
        private void ExportToExcelButton_Click(object sender, RoutedEventArgs e)
        {
            var reportData = ReportDataGrid.ItemsSource as IEnumerable<object>;
            if (reportData == null || !reportData.Any())
            {
                MessageBox.Show("Нет данных для экспорта.");
                return;
            }

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Отчет");


            var headers = ReportDataGrid.Columns.Select(c => c.Header.ToString()).ToArray();
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }


            int row = 2;
            foreach (var item in reportData)
            {
                var properties = item.GetType().GetProperties();
                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cell(row, i + 1).Value = properties[i].GetValue(item)?.ToString();
                }
                row++;
            }


            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = "Отчет.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                workbook.SaveAs(saveFileDialog.FileName);
                MessageBox.Show("Данные успешно экспортированы.");
            }
        }

        // Экспорт в Word
        private void ExportToWordButton_Click(object sender, RoutedEventArgs e)
        {
            var reportData = ReportDataGrid.ItemsSource as IEnumerable<object>;
            if (reportData == null || !reportData.Any())
            {
                MessageBox.Show("Нет данных для экспорта.");
                return;
            }

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Word files (*.docx)|*.docx",
                FileName = "Отчет.docx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(saveFileDialog.FileName, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
                {
                    var mainPart = wordDoc.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    var body = new Body();


                    var headers = ReportDataGrid.Columns.Select(c => c.Header.ToString()).ToArray();
                    var headerParagraph = new Paragraph();
                    var headerRun = new Run(new Text(string.Join("\t", headers)));
                    headerParagraph.Append(headerRun);
                    body.Append(headerParagraph);


                    foreach (var item in reportData)
                    {
                        var properties = item.GetType().GetProperties();
                        var rowData = string.Join("\t", properties.Select(p => p.GetValue(item)?.ToString()));
                        var dataParagraph = new Paragraph();
                        var dataRun = new Run(new Text(rowData));
                        dataParagraph.Append(dataRun);
                        body.Append(dataParagraph);
                    }

                    mainPart.Document.Append(body);
                }

                MessageBox.Show("Данные успешно экспортированы.");
            }
        }
    }
}