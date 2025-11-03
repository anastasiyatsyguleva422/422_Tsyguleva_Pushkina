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
using System.Windows.Shapes;

namespace _422_Tsyguleva_Pushkina.Pages
{
    /// <summary>
    /// Страница для добавления и редактирования платежей
    /// Предоставляет интерфейс для ввода данных о платежах и их сохранения в базе данных
    /// </summary>
    public partial class AddPaymentPage : Page
    {
        // Текущий платеж для работы (новый или редактируемый)
        private Payment _currentPayment = new Payment();

        /// <summary>
        /// Конструктор по умолчанию для создания нового платежа
        /// </summary>
        public AddPaymentPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Конструктор для редактирования существующего платежа
        /// </summary>
        /// <param name="selectedPayment">Выбранный платеж для редактирования</param>
        public AddPaymentPage(Payment selectedPayment)
        {
            InitializeComponent();

            // Инициализация выпадающих списков категорий и пользователей
            CBCategory.ItemsSource = DbContextHelper.GetContext().Category.ToList();
            CBCategory.DisplayMemberPath = "Name"; // Отображение названия категории

            CBUser.ItemsSource = DbContextHelper.GetContext().User.ToList();
            CBUser.DisplayMemberPath = "FIO"; // Отображение ФИО пользователя

            // Установка текущего платежа (переданный или новый)
            if (selectedPayment != null)
                _currentPayment = selectedPayment;

            // Привязка данных к элементам управления
            DataContext = _currentPayment;
        }

        /// <summary>
        /// Обработчик события сохранения платежа
        /// Выполняет валидацию данных и сохраняет платеж в базу данных
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            // StringBuilder для накопления ошибок валидации
            StringBuilder errors = new StringBuilder();

            // Проверка обязательных полей на заполнение
            if (string.IsNullOrWhiteSpace(_currentPayment.Date.ToString()))
                errors.AppendLine("Укажите дату!");
            if (string.IsNullOrWhiteSpace(_currentPayment.Num.ToString()))
                errors.AppendLine("Укажите количество!");
            if (string.IsNullOrWhiteSpace(_currentPayment.Price.ToString()))
                errors.AppendLine("Укажите цену");
            if (string.IsNullOrWhiteSpace(_currentPayment.UserID.ToString()))
                errors.AppendLine("Укажите клиента!");
            if (string.IsNullOrWhiteSpace(_currentPayment.CategoryID.ToString()))
                errors.AppendLine("Укажите категорию!");

            // Если есть ошибки валидации - показываем и прерываем выполнение
            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            // Определение типа операции: добавление нового платежа
            if (_currentPayment.ID == 0)
                DbContextHelper.GetContext().Payment.Add(_currentPayment);

            try
            {
                // Сохранение изменений в базе данных
                DbContextHelper.GetContext().SaveChanges();
                MessageBox.Show("Данные успешно сохранены!");
            }
            catch (Exception ex)
            {
                // Обработка исключений при работе с базой данных
                MessageBox.Show(ex.Message.ToString());
            }
        }

        /// <summary>
        /// Обработчик события очистки полей формы
        /// Сбрасывает значения текстовых полей ввода
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            // Очистка всех текстовых полей формы
            TBPaymentName.Text = "";
            TBAmount.Text = "";
            TBCount.Text = "";
            TBDate.Text = "";
        }
    }
}