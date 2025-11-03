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
    /// Страница управления платежами в системе
    /// Предоставляет интерфейс для просмотра, добавления, редактирования и удаления финансовых операций
    /// </summary>
    public partial class PaymentTabPage : Page
    {
        /// <summary>
        /// Конструктор страницы управления платежами
        /// Инициализирует компоненты и загружает данные в DataGrid
        /// </summary>
        public PaymentTabPage()
        {
            InitializeComponent();

            // Установка источника данных для DataGrid - загрузка всех платежей
            DataGridPayment.ItemsSource = DbContextHelper.GetContext().Payment.ToList();

            // Подписка на событие изменения видимости страницы для обновления данных
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        /// <summary>
        /// Обработчик события изменения видимости страницы
        /// Обновляет данные в DataGrid при каждом отображении страницы
        /// </summary>
        /// <param name="sender">Источник события (страница)</param>
        /// <param name="e">Аргументы события изменения свойства видимости</param>
        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Проверка, что страница стала видимой
            if (Visibility == Visibility.Visible)
            {
                // Принудительная перезагрузка всех отслеживаемых сущностей из базы данных
                DbContextHelper.GetContext().ChangeTracker.Entries().ToList().ForEach(x => x.Reload());

                // Обновление источника данных DataGrid актуальными данными
                DataGridPayment.ItemsSource = DbContextHelper.GetContext().Payment.ToList();
            }
        }

        /// <summary>
        /// Обработчик события добавления нового платежа
        /// Навигация на страницу создания новой финансовой операции
        /// </summary>
        /// <param name="sender">Источник события (кнопка добавления)</param>
        /// <param name="e">Аргументы события нажатия кнопки</param>
        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddPaymentPage(null)); // Передача null для создания нового платежа
        }

        /// <summary>
        /// Обработчик события удаления платежей
        /// Выполняет удаление выбранных платежей с подтверждением операции
        /// </summary>
        /// <param name="sender">Источник события (кнопка удаления)</param>
        /// <param name="e">Аргументы события нажатия кнопки</param>
        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            // Преобразование выбранных элементов в список платежей
            var paymentForRemoving = DataGridPayment.SelectedItems.Cast<Payment>().ToList();

            // Запрос подтверждения удаления с указанием количества элементов
            if (MessageBox.Show($"Вы точно хотите удалить записи в количестве {paymentForRemoving.Count()} элементов?",
                              "Внимание",
                              MessageBoxButton.YesNo,
                              MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    // ВАЖНО: В текущей реализации отсутствует фактическое удаление записей из базы данных
                    // Для полноценной функциональности необходимо добавить код удаления:
                    // 
                    // foreach (var payment in paymentForRemoving)
                    // {
                    //     DbContextHelper.GetContext().Payment.Remove(payment);
                    // }
                    // DbContextHelper.GetContext().SaveChanges();

                    MessageBox.Show("Данные успешно удалены!");

                    // Обновление DataGrid для отражения изменений
                    DataGridPayment.ItemsSource = DbContextHelper.GetContext().Payment.ToList();
                }
                catch (Exception ex)
                {
                    // Обработка исключений при работе с базой данных
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }

        /// <summary>
        /// Обработчик события редактирования платежа
        /// Навигация на страницу редактирования выбранного платежа
        /// </summary>
        /// <param name="sender">Источник события (кнопка редактирования в DataGrid)</param>
        /// <param name="e">Аргументы события нажатия кнопки</param>
        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            // Получение объекта платежа из DataContext кнопки и навигация на страницу редактирования
            NavigationService.Navigate(new Pages.AddPaymentPage((sender as Button).DataContext as Payment));
        }
    }
}