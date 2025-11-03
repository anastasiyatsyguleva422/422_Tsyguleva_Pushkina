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
    /// Страница управления пользователями для роли "User"
    /// Предоставляет функциональность просмотра, фильтрации и сортировки списка пользователей системы
    /// </summary>
    public partial class UserPage : Page
    {
        /// <summary>
        /// Конструктор страницы пользователей
        /// Инициализирует компоненты и загружает начальный список пользователей
        /// </summary>
        public UserPage()
        {
            InitializeComponent();

            // Загрузка списка пользователей из базы данных
            var currentUsers = DbContextHelper.GetContext().User.ToList();
            ListUser.ItemsSource = currentUsers;
        }

        /// <summary>
        /// Обработчик события сброса фильтров
        /// Восстанавливает исходное состояние всех элементов фильтрации и сортировки
        /// </summary>
        /// <param name="sender">Источник события (кнопка сброса фильтров)</param>
        /// <param name="e">Аргументы события нажатия кнопки</param>
        private void clearFiltersButton_Click_1(object sender, RoutedEventArgs e)
        {
            // Сброс значений фильтров к состоянию по умолчанию
            fioFilterTextBox.Text = "";
            sortComboBox.SelectedIndex = 0;
            onlyAdminCheckBox.IsChecked = false;
        }

        /// <summary>
        /// Обработчик изменения текста в поле фильтрации по ФИО
        /// Автоматически обновляет список пользователей при изменении текста фильтра
        /// </summary>
        /// <param name="sender">Источник события (поле ввода ФИО)</param>
        /// <param name="e">Аргументы события изменения текста</param>
        private void fioFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateUsers();
        }

        /// <summary>
        /// Обработчик изменения выбора в комбобоксе сортировки
        /// Обновляет порядок отображения пользователей согласно выбранному критерию сортировки
        /// </summary>
        /// <param name="sender">Источник события (комбобокс сортировки)</param>
        /// <param name="e">Аргументы события изменения выбора</param>
        private void sortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUsers();
        }

        /// <summary>
        /// Обработчик события установки флажка "Только администраторы"
        /// Фильтрует список пользователей, оставляя только пользователей с ролью "Admin"
        /// </summary>
        /// <param name="sender">Источник события (флажок фильтра)</param>
        /// <param name="e">Аргументы события установки флажка</param>
        private void onlyAdminCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateUsers();
        }

        /// <summary>
        /// Обработчик события снятия флажка "Только администраторы"
        /// Восстанавливает отображение всех пользователей независимо от роли
        /// </summary>
        /// <param name="sender">Источник события (флажок фильтра)</param>
        /// <param name="e">Аргументы события снятия флажка</param>
        private void onlyAdminCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateUsers();
        }

        /// <summary>
        /// Основной метод обновления отображаемого списка пользователей
        /// Применяет все активные фильтры и сортировку к исходному набору данных
        /// </summary>
        private void UpdateUsers()
        {
            // Проверка инициализации компонентов для предотвращения выполнения во время загрузки
            if (!IsInitialized)
            {
                return;
            }

            try
            {
                // Получение полного списка пользователей из базы данных
                List<User> currentUsers = DbContextHelper.GetContext().User.ToList();

                // Применение фильтрации по ФИО (регистронезависимый поиск)
                if (!string.IsNullOrWhiteSpace(fioFilterTextBox.Text))
                {
                    currentUsers = currentUsers.Where(x =>
                   x.FIO.ToLower().Contains(fioFilterTextBox.Text.ToLower())).ToList();
                }

                // Применение фильтрации по роли "Admin"
                if (onlyAdminCheckBox.IsChecked.Value)
                {
                    currentUsers = currentUsers.Where(x => x.Role == "Admin").ToList();
                }

                // Применение сортировки в зависимости от выбранного варианта
                // 0 - сортировка по ФИО по возрастанию (А-Я)
                // 1 - сортировка по ФИО по убыванию (Я-А)
                ListUser.ItemsSource = (sortComboBox.SelectedIndex == 0)
                    ? currentUsers.OrderBy(x => x.FIO).ToList()
                    : currentUsers.OrderByDescending(x => x.FIO).ToList();
            }
            catch (Exception)
            {
                // Тихая обработка исключений - в реальном приложении следует добавить логирование
                // для отслеживания проблем с доступом к базе данных или фильтрацией
            }
        }
    }
}