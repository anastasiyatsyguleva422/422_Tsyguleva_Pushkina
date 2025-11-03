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
    /// <summary>
    /// Страница визуализации данных и экспорта отчетов
    /// Предоставляет функциональность построения диаграмм платежей и экспорта данных в Excel/Word форматы
    /// </summary>
    public partial class DiagrammPage : System.Windows.Controls.Page
    {
        // Контекст базы данных для работы с сущностями платежей
        private Tsyguleva_Pushkina_DB_PaymentEntities _context = new Tsyguleva_Pushkina_DB_PaymentEntities();

        // Компонент Chart для визуализации данных
        private System.Windows.Forms.DataVisualization.Charting.Chart paymentChart;

        /// <summary>
        /// Конструктор страницы диаграмм
        /// Инициализирует компоненты, загружает данные и настраивает обработчики событий
        /// </summary>
        public DiagrammPage()
        {
            InitializeComponent();

            // Инициализация компонента диаграммы
            InitializeChart();

            // Загрузка данных в интерфейс
            LoadUsersFromDatabase();
            LoadChartTypes();

            // Подписка на события изменения выбора
            UserComboBox.SelectionChanged += UpdateChart;
            ChartTypeComboBox.SelectionChanged += UpdateChart;

            // Подписка на события кнопок экспорта
            ExportToExcelButton.Click += ExportToExcelButton_Click;
            ExportToWordButton.Click += ExportToWordButton_Click;

            // Первоначальное обновление диаграммы
            UpdateChart(null, null);
        }

        /// <summary>
        /// Инициализирует компонент Chart для отображения диаграмм
        /// Настраивает область построения и легенду
        /// </summary>
        private void InitializeChart()
        {
            // Создание экземпляра компонента Chart
            paymentChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            paymentChart.Dock = System.Windows.Forms.DockStyle.Fill; // Заполнение всего доступного пространства
            paymentChart.BackColor = System.Drawing.Color.White; // Установка белого фона

            // Создание и настройка области диаграммы
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            chartArea.BackColor = System.Drawing.Color.White;
            paymentChart.ChartAreas.Add(chartArea);

            // Создание и добавление легенды
            System.Windows.Forms.DataVisualization.Charting.Legend legend = new System.Windows.Forms.DataVisualization.Charting.Legend();
            paymentChart.Legends.Add(legend);

            // Размещение компонента в контейнере WindowsFormsHost
            ChartHost.Child = paymentChart;
        }

        /// <summary>
        /// Загружает список пользователей из базы данных в ComboBox
        /// Обрабатывает возможные ошибки при загрузке данных
        /// </summary>
        private void LoadUsersFromDatabase()
        {
            try
            {
                // Получение списка пользователей из базы данных
                var users = _context.User.ToList();
                UserComboBox.ItemsSource = users;

                // Установка первого пользователя по умолчанию, если список не пуст
                if (users.Count > 0)
                    UserComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}");
            }
        }

        /// <summary>
        /// Загружает доступные типы диаграмм в ComboBox
        /// Предоставляет выбор между круговой, столбчатой диаграммой и линейным графиком
        /// </summary>
        private void LoadChartTypes()
        {
            // Список доступных типов диаграмм
            var chartTypes = new List<string>
            {
                "Круговая диаграмма",
                "Столбчатая диаграмма",
                "Линейный график"
            };

            // Установка источника данных и выбор по умолчанию
            ChartTypeComboBox.ItemsSource = chartTypes;
            if (chartTypes.Count > 0)
                ChartTypeComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Обновляет диаграмму при изменении выбора пользователя или типа диаграммы
        /// Строит визуализацию платежей выбранного пользователя по категориям
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события изменения выбора</param>
        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            // Проверка наличия выбранного пользователя и типа диаграммы
            if (UserComboBox.SelectedItem is User currentUser && ChartTypeComboBox.SelectedItem != null)
            {
                try
                {
                    // Очистка предыдущих данных диаграммы
                    paymentChart.Series.Clear();
                    paymentChart.Titles.Clear();

                    // Создание новой серии данных
                    var series = new System.Windows.Forms.DataVisualization.Charting.Series("Платежи");

                    // Установка типа диаграммы в зависимости от выбора пользователя
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

                    // Настройка отображения значений на диаграмме
                    series.IsValueShownAsLabel = true;

                    // Получение всех категорий для группировки платежей
                    var categories = _context.Category.ToList();

                    // Заполнение данными для каждой категории
                    foreach (var category in categories)
                    {
                        // Получение платежей пользователя по текущей категории
                        var payments = _context.Payment
                            .Where(p => p.UserID == currentUser.ID && p.CategoryID == category.ID)
                            .ToList();

                        // Расчет общей суммы платежей по категории
                        double sum = payments.Sum(p => (double)(p.Price * p.Num));

                        // Добавление точки данных на диаграмму
                        series.Points.AddXY(category.Name, sum);
                    }

                    // Добавление серии на диаграмму и установка заголовка
                    paymentChart.Series.Add(series);
                    paymentChart.Titles.Add($"Платежи: {currentUser.FIO}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления диаграммы: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Обработчик экспорта данных в Excel формат
        /// Создает структурированный отчет с детализацией платежей по пользователям
        /// </summary>
        /// <param name="sender">Источник события (кнопка экспорта)</param>
        /// <param name="e">Аргументы события нажатия кнопки</param>
        private void ExportToExcelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получение и сортировка списка пользователей по ФИО
                var allUsers = _context.User.ToList().OrderBy(u => u.FIO).ToList();

                // Создание экземпляра приложения Excel и рабочей книги
                var application = new Excel.Application();
                Excel.Workbook workbook = application.Workbooks.Add();

                // Удаление лишних листов, оставляем только один
                while (workbook.Worksheets.Count > 1)
                {
                    workbook.Worksheets[2].Delete();
                }

                // Настройка основного рабочего листа
                Excel.Worksheet worksheet = workbook.Worksheets[1];
                worksheet.Name = "Платежи";

                decimal grandTotal = 0; // Общий итог по всем пользователям
                int currentRow = 1; // Текущая строка для записи данных

                // Обработка данных для каждого пользователя
                foreach (var user in allUsers)
                {
                    // Заголовок раздела пользователя
                    worksheet.Cells[currentRow, 1] = $"Пользователь: {user.FIO}";
                    Excel.Range userHeaderRange = worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, 5]];
                    userHeaderRange.Merge();
                    userHeaderRange.Font.Bold = true;
                    userHeaderRange.Font.Size = 14;
                    userHeaderRange.Interior.Color = Excel.XlRgbColor.rgbLightGray;
                    currentRow++;
                    currentRow++; // Пустая строка для визуального разделения

                    // Заголовки столбцов таблицы
                    worksheet.Cells[currentRow, 1] = "Дата платежа";
                    worksheet.Cells[currentRow, 2] = "Категория";
                    worksheet.Cells[currentRow, 3] = "Название";
                    worksheet.Cells[currentRow, 4] = "Стоимость";
                    worksheet.Cells[currentRow, 5] = "Количество";
                    worksheet.Cells[currentRow, 6] = "Сумма";

                    // Форматирование заголовков столбцов
                    Excel.Range columnHeaderRange = worksheet.Range[worksheet.Cells[currentRow, 1], worksheet.Cells[currentRow, 6]];
                    columnHeaderRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    columnHeaderRange.Font.Bold = true;
                    columnHeaderRange.Interior.Color = Excel.XlRgbColor.rgbLightBlue;
                    currentRow++;

                    // Получение платежей текущего пользователя
                    var userPayments = _context.Payment
                        .Where(p => p.UserID == user.ID)
                        .OrderBy(p => p.Date)
                        .ToList();

                    decimal userTotal = 0; // Итог по текущему пользователю

                    // Запись данных платежей в таблицу
                    foreach (var payment in userPayments)
                    {
                        worksheet.Cells[currentRow, 1] = payment.Date.ToString("dd.MM.yyyy");
                        worksheet.Cells[currentRow, 2] = payment.Category?.Name ?? "Без категории";
                        worksheet.Cells[currentRow, 3] = payment.Name;
                        worksheet.Cells[currentRow, 4] = payment.Price;
                        (worksheet.Cells[currentRow, 4] as Excel.Range).NumberFormat = "0.00"; // Формат числа
                        worksheet.Cells[currentRow, 5] = payment.Num;

                        // Расчет и запись суммы платежа
                        decimal paymentTotal = payment.Price * payment.Num;
                        worksheet.Cells[currentRow, 6] = paymentTotal;
                        (worksheet.Cells[currentRow, 6] as Excel.Range).NumberFormat = "0.00";

                        userTotal += paymentTotal; // Накопление итога пользователя
                        currentRow++;
                    }

                    // Добавление строки итога для пользователя, если есть платежи
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

                        grandTotal += userTotal; // Добавление к общему итогу
                        currentRow++;
                        currentRow++; // Пустая строка для разделения
                    }
                }

                // Добавление общего итога, если пользователей больше одного
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

                // Добавление границ для таблицы данных
                if (currentRow > 1)
                {
                    Excel.Range dataRange = worksheet.Range[worksheet.Cells[3, 1], worksheet.Cells[currentRow, 6]];
                    dataRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                }

                // Автоматическое выравнивание ширины столбцов
                worksheet.Columns.AutoFit();

                // Отображение приложения Excel
                application.Visible = true;

                // Сохранение файла на рабочий стол
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string excelPath = System.IO.Path.Combine(desktopPath, "Payments_Report.xlsx");

                // Удаление существующего файла, если он есть
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

        /// <summary>
        /// Обработчик экспорта данных в Word и PDF форматы
        /// Создает структурированный отчет с таблицами и аналитикой по платежам
        /// </summary>
        /// <param name="sender">Источник события (кнопка экспорта)</param>
        /// <param name="e">Аргументы события нажатия кнопки</param>
        private void ExportToWordButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получение данных из базы данных
                var allUsers = _context.User.ToList();
                var allCategories = _context.Category.ToList();

                // Создание экземпляра приложения Word и документа
                var application = new Word.Application();
                Word.Document document = application.Documents.Add();

                // Настройка верхнего колонтитула с датой
                foreach (Word.Section section in document.Sections)
                {
                    Word.Range headerRange = section.Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    headerRange.Fields.Add(headerRange, Word.WdFieldType.wdFieldDate);
                    headerRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    headerRange.Font.ColorIndex = Word.WdColorIndex.wdBlack;
                    headerRange.Font.Size = 10;
                    headerRange.Text = "Отчет по платежам - " + DateTime.Now.ToString("dd/MM/yyyy");
                }

                // Настройка нижнего колонтитула с номерами страниц
                foreach (Word.Section section in document.Sections)
                {
                    Word.HeaderFooter footer = section.Footers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                    footer.PageNumbers.Add(Word.WdPageNumberAlignment.wdAlignPageNumberCenter);
                }

                // Создание отчета для каждого пользователя
                foreach (var user in allUsers)
                {
                    // Заголовок раздела пользователя
                    Word.Paragraph userParagraph = document.Paragraphs.Add();
                    Word.Range userRange = userParagraph.Range;
                    userRange.Text = user.FIO;

                    // Применение стилей заголовка с обработкой возможных исключений
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
                            // Ручное форматирование при отсутствии стилей
                            userRange.Font.Size = 16;
                            userRange.Font.Bold = 1;
                        }
                    }

                    userRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    userRange.InsertParagraphAfter();
                    document.Paragraphs.Add(); // Пустая строка для разделения

                    // Создание таблицы для отображения платежей по категориям
                    Word.Paragraph tableParagraph = document.Paragraphs.Add();
                    Word.Range tableRange = tableParagraph.Range;
                    Word.Table paymentsTable = document.Tables.Add(tableRange, allCategories.Count() + 1, 2);
                    paymentsTable.Borders.InsideLineStyle = paymentsTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                    paymentsTable.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                    Word.Range cellRange;

                    // Заголовки столбцов таблицы
                    cellRange = paymentsTable.Cell(1, 1).Range;
                    cellRange.Text = "Категория";
                    cellRange = paymentsTable.Cell(1, 2).Range;
                    cellRange.Text = "Сумма расходов";

                    // Форматирование заголовков таблицы
                    paymentsTable.Rows[1].Range.Font.Name = "Times New Roman";
                    paymentsTable.Rows[1].Range.Font.Size = 14;
                    paymentsTable.Rows[1].Range.Bold = 1;
                    paymentsTable.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    // Заполнение таблицы данными по категориям
                    for (int i = 0; i < allCategories.Count(); i++)
                    {
                        var currentCategory = allCategories[i];

                        // Название категории
                        cellRange = paymentsTable.Cell(i + 2, 1).Range;
                        cellRange.Text = currentCategory.Name;
                        cellRange.Font.Name = "Times New Roman";
                        cellRange.Font.Size = 12;

                        // Сумма расходов по категории
                        cellRange = paymentsTable.Cell(i + 2, 2).Range;

                        // Расчет общей суммы платежей по категории
                        var categoryPayments = _context.Payment
                            .Where(p => p.UserID == user.ID && p.CategoryID == currentCategory.ID)
                            .ToList();

                        decimal totalAmount = categoryPayments.Sum(p => p.Price * p.Num);
                        cellRange.Text = totalAmount.ToString("N2") + " руб.";
                        cellRange.Font.Name = "Times New Roman";
                        cellRange.Font.Size = 12;
                    }

                    document.Paragraphs.Add(); // Пустая строка после таблицы

                    // Аналитика платежей пользователя
                    var userPayments = _context.Payment.Where(p => p.UserID == user.ID).ToList();
                    if (userPayments.Any())
                    {
                        // Поиск самого дорогого платежа
                        Payment maxPayment = userPayments.OrderByDescending(p => p.Price * p.Num).FirstOrDefault();
                        if (maxPayment != null)
                        {
                            Word.Paragraph maxPaymentParagraph = document.Paragraphs.Add();
                            Word.Range maxPaymentRange = maxPaymentParagraph.Range;
                            maxPaymentRange.Text = $"Самый дорогостоящий платеж - {maxPayment.Name} за {(maxPayment.Price * maxPayment.Num).ToString("N2")} руб. от {maxPayment.Date.ToString("dd.MM.yyyy")}";

                            // Применение стилей для аналитики
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

                        // Поиск самого дешевого платежа
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

                    document.Paragraphs.Add(); // Пустая строка в конце раздела

                    // Добавление разрыва страницы для следующего пользователя (кроме последнего)
                    if (user != allUsers.LastOrDefault())
                    {
                        document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                    }
                }

                // Отображение приложения Word
                application.Visible = true;

                // Сохранение документов в разных форматах
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string docxPath = System.IO.Path.Combine(desktopPath, "Payments.docx");
                string pdfPath = System.IO.Path.Combine(desktopPath, "Payments.pdf");

                document.SaveAs2(docxPath); // Сохранение в Word формате
                document.SaveAs2(pdfPath, Word.WdExportFormat.wdExportFormatPDF); // Сохранение в PDF формате

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