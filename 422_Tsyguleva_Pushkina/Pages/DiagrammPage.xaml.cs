using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Threading;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;

namespace _422_Tsyguleva_Pushkina.Pages
{
    public partial class DiagrammPage : System.Windows.Controls.Page
    {
        private Tsyguleva_Pushkina_DB_PaymentEntities _context = new Tsyguleva_Pushkina_DB_PaymentEntities();
        private System.Windows.Forms.DataVisualization.Charting.Chart paymentChart;

        public DiagrammPage()
        {
            InitializeComponent();
            InitializeChart();

            LoadUsersFromDatabase();
            LoadChartTypes();

            UserComboBox.SelectionChanged += UpdateChart;
            ChartTypeComboBox.SelectionChanged += UpdateChart;
            ExportToExcelButton.Click += ExportToExcelButton_Click;
            ExportToWordButton.Click += ExportToWordButton_Click;

            UpdateChart(null, null);
        }

        private void InitializeChart()
        {
            paymentChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            paymentChart.Dock = System.Windows.Forms.DockStyle.Fill;
            paymentChart.BackColor = System.Drawing.Color.White;

            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            chartArea.BackColor = System.Drawing.Color.White;
            paymentChart.ChartAreas.Add(chartArea);

            System.Windows.Forms.DataVisualization.Charting.Legend legend = new System.Windows.Forms.DataVisualization.Charting.Legend();
            paymentChart.Legends.Add(legend);

            ChartHost.Child = paymentChart;
        }

        private void LoadUsersFromDatabase()
        {
            try
            {
                var users = _context.User.ToList();
                UserComboBox.ItemsSource = users;
                if (users.Count > 0)
                    UserComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}");
            }
        }

        private void LoadChartTypes()
        {
            var chartTypes = new List<string>
            {
                "Круговая диаграмма",
                "Столбчатая диаграмма",
                "Линейный график"
            };

            ChartTypeComboBox.ItemsSource = chartTypes;
            if (chartTypes.Count > 0)
                ChartTypeComboBox.SelectedIndex = 0;
        }

        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            if (UserComboBox.SelectedItem is User currentUser && ChartTypeComboBox.SelectedItem != null)
            {
                try
                {
                    paymentChart.Series.Clear();
                    paymentChart.Titles.Clear();

                    var series = new System.Windows.Forms.DataVisualization.Charting.Series("Платежи");

                    switch (ChartTypeComboBox.SelectedItem.ToString())
                    {
                        case "Круговая диаграмма":
                            series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
                            break;
                        case "Столбчатая диаграмма":
                            series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
                            break;
                        case "Линейный график":
                            series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                            break;
                    }

                    series.IsValueShownAsLabel = true;

                    var categories = _context.Category.ToList();
                    foreach (var category in categories)
                    {
                        var payments = _context.Payment
                            .Where(p => p.UserID == currentUser.ID && p.CategoryID == category.ID)
                            .ToList();

                        double sum = payments.Sum(p => (double)(p.Price * p.Num));
                        series.Points.AddXY(category.Name, sum);
                    }

                    paymentChart.Series.Add(series);
                    paymentChart.Titles.Add($"Платежи: {currentUser.FIO}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления диаграммы: {ex.Message}");
                }
            }
        }

        private void ExportToExcelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var allUsers = _context.User.ToList().OrderBy(u => u.FIO).ToList();

                var application = new Excel.Application();
                Excel.Workbook workbook = application.Workbooks.Add();

                while (workbook.Worksheets.Count > 1)
                {
                    workbook.Worksheets[2].Delete();
                }

                Excel.Worksheet worksheet = workbook.Worksheets[1];
                worksheet.Name = "Платежи";

                decimal grandTotal = 0; 
                int currentRow = 1;

                foreach (var user in allUsers)
                {
                    worksheet.Cells[currentRow, 1] = $"Пользователь: {user.FIO}";
                    Excel.Range userHeaderRange = worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, 5]];
                    userHeaderRange.Merge();
                    userHeaderRange.Font.Bold = true;
                    userHeaderRange.Font.Size = 14;
                    userHeaderRange.Interior.Color = Excel.XlRgbColor.rgbLightGray;
                    currentRow++;
                    currentRow++; 

                    worksheet.Cells[currentRow, 1] = "Дата платежа";
                    worksheet.Cells[currentRow, 2] = "Категория";
                    worksheet.Cells[currentRow, 3] = "Название";
                    worksheet.Cells[currentRow, 4] = "Стоимость";
                    worksheet.Cells[currentRow, 5] = "Количество";
                    worksheet.Cells[currentRow, 6] = "Сумма";

                    Excel.Range columnHeaderRange = worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, 6]];
                    columnHeaderRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    columnHeaderRange.Font.Bold = true;
                    columnHeaderRange.Interior.Color = Excel.XlRgbColor.rgbLightBlue;
                    currentRow++;
                    var userPayments = _context.Payment
                        .Where(p => p.UserID == user.ID)
                        .OrderBy(p => p.Date)
                        .ToList();

                    decimal userTotal = 0;
                    foreach (var payment in userPayments)
                    {
                        worksheet.Cells[currentRow, 1] = payment.Date.ToString("dd.MM.yyyy");
                        worksheet.Cells[currentRow, 2] = payment.Category?.Name ?? "Без категории";
                        worksheet.Cells[currentRow, 3] = payment.Name;
                        worksheet.Cells[currentRow, 4] = payment.Price;
                        (worksheet.Cells[currentRow, 4] as Excel.Range).NumberFormat = "0.00";
                        worksheet.Cells[currentRow, 5] = payment.Num;

                        decimal paymentTotal = payment.Price * payment.Num;
                        worksheet.Cells[currentRow, 6] = paymentTotal;
                        (worksheet.Cells[currentRow, 6] as Excel.Range).NumberFormat = "0.00";

                        userTotal += paymentTotal;
                        currentRow++;
                    }

                    if (userPayments.Any())
                    {
                        Excel.Range userTotalRange = worksheet.Range[
                            worksheet.Cells[currentRow, 1],
                            worksheet.Cells[currentRow, 5]];
                        userTotalRange.Merge();
                        userTotalRange.Value = $"ИТОГО для {user.FIO}:";
                        userTotalRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                        userTotalRange.Font.Bold = true;

                        worksheet.Cells[currentRow, 6] = userTotal;
                        (worksheet.Cells[currentRow, 6] as Excel.Range).NumberFormat = "0.00";
                        (worksheet.Cells[currentRow, 6] as Excel.Range).Font.Bold = true;
                        userTotalRange.Interior.Color = Excel.XlRgbColor.rgbLightGreen;

                        grandTotal += userTotal;
                        currentRow++;
                        currentRow++; 
                    }
                }
                if (allUsers.Count > 1)
                {
                    Excel.Range grandTotalRange = worksheet.Range[
                        worksheet.Cells[currentRow, 1],
                        worksheet.Cells[currentRow, 5]];
                    grandTotalRange.Merge();
                    grandTotalRange.Value = "ОБЩИЙ ИТОГ ПО ВСЕМ ПОЛЬЗОВАТЕЛЯМ:";
                    grandTotalRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                    grandTotalRange.Font.Bold = true;
                    grandTotalRange.Font.Color = Excel.XlRgbColor.rgbRed;

                    worksheet.Cells[currentRow, 6] = grandTotal;
                    (worksheet.Cells[currentRow, 6] as Excel.Range).NumberFormat = "0.00";
                    (worksheet.Cells[currentRow, 6] as Excel.Range).Font.Bold = true;
                    (worksheet.Cells[currentRow, 6] as Excel.Range).Font.Color = Excel.XlRgbColor.rgbRed;
                    (worksheet.Cells[currentRow, 6] as Excel.Range).Interior.Color = Excel.XlRgbColor.rgbLightYellow;
                }
                if (currentRow > 1)
                {
                    Excel.Range dataRange = worksheet.Range[worksheet.Cells[3, 1], worksheet.Cells[currentRow, 6]];
                    dataRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                }
                worksheet.Columns.AutoFit();

                application.Visible = true;

                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string excelPath = System.IO.Path.Combine(desktopPath, "Payments_Report.xlsx");

                if (System.IO.File.Exists(excelPath))
                {
                    System.IO.File.Delete(excelPath);
                }

                workbook.SaveAs(excelPath);

                MessageBox.Show($"Excel файл успешно сохранен:\n{excelPath}",
                    "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}\n\nДетали: {ex.InnerException?.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ExportToWordButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var allUsers = _context.User.ToList();
                var allCategories = _context.Category.ToList();

                var application = new Word.Application();
                Word.Document document = application.Documents.Add();

                foreach (Word.Section section in document.Sections)
                {
                    Word.Range headerRange = section.Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    headerRange.Fields.Add(headerRange, Word.WdFieldType.wdFieldDate);
                    headerRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    headerRange.Font.ColorIndex = Word.WdColorIndex.wdBlack;
                    headerRange.Font.Size = 10;
                    headerRange.Text = "Отчет по платежам - " + DateTime.Now.ToString("dd/MM/yyyy");
                }

                foreach (Word.Section section in document.Sections)
                {
                    Word.HeaderFooter footer = section.Footers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                    footer.PageNumbers.Add(Word.WdPageNumberAlignment.wdAlignPageNumberCenter);
                }

                foreach (var user in allUsers)
                {
                    Word.Paragraph userParagraph = document.Paragraphs.Add();
                    Word.Range userRange = userParagraph.Range;
                    userRange.Text = user.FIO;

                    try
                    {
                        userParagraph.set_Style("Заголовок");
                    }
                    catch
                    {
                        try
                        {
                            userParagraph.set_Style("Заголовок 1");
                        }
                        catch
                        {
                            userRange.Font.Size = 16;
                            userRange.Font.Bold = 1;
                        }
                    }

                    userRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    userRange.InsertParagraphAfter();
                    document.Paragraphs.Add(); 

                    Word.Paragraph tableParagraph = document.Paragraphs.Add();
                    Word.Range tableRange = tableParagraph.Range;
                    Word.Table paymentsTable = document.Tables.Add(tableRange, allCategories.Count() + 1, 2);
                    paymentsTable.Borders.InsideLineStyle = paymentsTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                    paymentsTable.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                    Word.Range cellRange;

                    cellRange = paymentsTable.Cell(1, 1).Range;
                    cellRange.Text = "Категория";
                    cellRange = paymentsTable.Cell(1, 2).Range;
                    cellRange.Text = "Сумма расходов";

                    paymentsTable.Rows[1].Range.Font.Name = "Times New Roman";
                    paymentsTable.Rows[1].Range.Font.Size = 14;
                    paymentsTable.Rows[1].Range.Bold = 1;
                    paymentsTable.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    for (int i = 0; i < allCategories.Count(); i++)
                    {
                        var currentCategory = allCategories[i];

                        cellRange = paymentsTable.Cell(i + 2, 1).Range;
                        cellRange.Text = currentCategory.Name;
                        cellRange.Font.Name = "Times New Roman";
                        cellRange.Font.Size = 12;

                        cellRange = paymentsTable.Cell(i + 2, 2).Range;

                        var categoryPayments = _context.Payment
                            .Where(p => p.UserID == user.ID && p.CategoryID == currentCategory.ID)
                            .ToList();

                        decimal totalAmount = categoryPayments.Sum(p => p.Price * p.Num);
                        cellRange.Text = totalAmount.ToString("N2") + " руб.";
                        cellRange.Font.Name = "Times New Roman";
                        cellRange.Font.Size = 12;
                    }

                    document.Paragraphs.Add(); 

                    var userPayments = _context.Payment.Where(p => p.UserID == user.ID).ToList();
                    if (userPayments.Any())
                    {
                        Payment maxPayment = userPayments.OrderByDescending(p => p.Price * p.Num).FirstOrDefault();
                        if (maxPayment != null)
                        {
                            Word.Paragraph maxPaymentParagraph = document.Paragraphs.Add();
                            Word.Range maxPaymentRange = maxPaymentParagraph.Range;
                            maxPaymentRange.Text = $"Самый дорогостоящий платеж - {maxPayment.Name} за {(maxPayment.Price * maxPayment.Num).ToString("N2")} руб. от {maxPayment.Date.ToString("dd.MM.yyyy")}";

                            try
                            {
                                maxPaymentParagraph.set_Style("Подзаголовок");
                            }
                            catch
                            {
                                maxPaymentRange.Font.Size = 12;
                                maxPaymentRange.Font.Bold = 1;
                            }

                            maxPaymentRange.Font.Color = Word.WdColor.wdColorDarkRed;
                            maxPaymentRange.InsertParagraphAfter();
                        }

                        Payment minPayment = userPayments.OrderBy(p => p.Price * p.Num).FirstOrDefault();
                        if (minPayment != null)
                        {
                            Word.Paragraph minPaymentParagraph = document.Paragraphs.Add();
                            Word.Range minPaymentRange = minPaymentParagraph.Range;
                            minPaymentRange.Text = $"Самый дешевый платеж - {minPayment.Name} за {(minPayment.Price * minPayment.Num).ToString("N2")} руб. от {minPayment.Date.ToString("dd.MM.yyyy")}";

                            try
                            {
                                minPaymentParagraph.set_Style("Подзаголовок");
                            }
                            catch
                            {
                                minPaymentRange.Font.Size = 12;
                                minPaymentRange.Font.Bold = 1;
                            }

                            minPaymentRange.Font.Color = Word.WdColor.wdColorDarkGreen;
                            minPaymentRange.InsertParagraphAfter();
                        }
                    }

                    document.Paragraphs.Add(); 

                    if (user != allUsers.LastOrDefault())
                    {
                        document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                    }
                }

                application.Visible = true;

                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string docxPath = System.IO.Path.Combine(desktopPath, "Payments.docx");
                string pdfPath = System.IO.Path.Combine(desktopPath, "Payments.pdf");

                document.SaveAs2(docxPath);
                document.SaveAs2(pdfPath, Word.WdExportFormat.wdExportFormatPDF);

                MessageBox.Show($"Документы успешно сохранены:\n{docxPath}\n{pdfPath}",
                    "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Word: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}