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
    /// Страница управления пользователями в административной панели
    /// Предоставляет функциональность просмотра, добавления, редактирования и удаления пользователей системы
    /// </summary>
    public partial class UsersTabPage : Page
    {
        /// <summary>
        /// Конструктор страницы управления пользователями
        /// Инициализирует компоненты и загружает данные пользователей в DataGrid
        /// </summary>
        public UsersTabPage()
        {
            InitializeComponent();

            // Загрузка списка пользователей в DataGrid при инициализации
            DataGridUser.ItemsSource = DbContextHelper.GetContext().User.ToList();

            // Подписка на событие изменения видимости страницы для обновления данных
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        /// <summary>
        /// Обновляет данные в DataGrid пользователей
        /// Перезагружает данные из базы данных и обновляет отображение DataGrid
        /// </summary>
        private void UpdateUsersData()
        {
            // Установка нового источника данных и принудительное обновление отображения
            DataGridUser.ItemsSource = DbContextHelper.GetContext().User.ToList();
            DataGridUser.Items.Refresh(); // Принудительное обновление визуального отображения
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
                DataGridUser.ItemsSource = DbContextHelper.GetContext().User.ToList();
            }
        }

        /// <summary>
        /// Обработчик события добавления нового пользователя
        /// Навигация на страницу создания новой учетной записи
        /// </summary>
        /// <param name="sender">Источник события (кнопка добавления)</param>
        /// <param name="e">Аргументы события нажатия кнопки</param>
        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddUserPage(null)); // Передача null для создания нового пользователя
        }

        /// <summary>
        /// Обработчик события удаления пользователей
        /// Выполняет удаление выбранных пользователей с подтверждением операции
        /// </summary>
        /// <param name="sender">Источник события (кнопка удаления)</param>
        /// <param name="e">Аргументы события нажатия кнопки</param>
        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            // Преобразование выбранных элементов в список пользователей
            var usersForRemoving = DataGridUser.SelectedItems.Cast<User>().ToList();

            // Запрос подтверждения удаления с указанием количества элементов
            if (MessageBox.Show($"Вы точно хотите удалить записи в количестве {usersForRemoving.Count()} элементов?",
                              "Внимание",
                              MessageBoxButton.YesNo,
                              MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    // ВАЖНО: В текущей реализации отсутствует фактическое удаление записей из базы данных
                    // Для полноценной функциональности необходимо добавить код удаления:
                    // 
                    // foreach (var user in usersForRemoving)
                    // {
                    //     DbContextHelper.GetContext().User.Remove(user);
                    // }
                    // DbContextHelper.GetContext().SaveChanges();

                    // Сообщение об успешном удалении (в текущей реализации - преждевременное)
                    MessageBox.Show("Данные успешно удалены!");

                    // Обновление DataGrid для отражения изменений
                    DataGridUser.ItemsSource = DbContextHelper.GetContext().User.ToList();
                }
                catch (Exception ex)
                {
                    // Обработка исключений при работе с базой данных
                    MessageBox.Show(ex.Message.ToString());
                }
                foreach (var user in usersForRemoving)
                {
                    DbContextHelper.GetContext().User.Remove(user);
                }
                DbContextHelper.GetContext().SaveChanges();
            }
        }

        /// <summary>
        /// Обработчик события редактирования пользователя
        /// Навигация на страницу редактирования выбранного пользователя
        /// </summary>
        /// <param name="sender">Источник события (кнопка редактирования в DataGrid)</param>
        /// <param name="e">Аргументы события нажатия кнопки</param>
        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            // Получение объекта пользователя из DataContext кнопки и навигация на страницу редактирования
            NavigationService.Navigate(new Pages.AddUserPage((sender as Button).DataContext as User));
        }
    }
}