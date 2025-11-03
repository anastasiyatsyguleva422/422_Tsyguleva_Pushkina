using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace _422_Tsyguleva_Pushkina.Pages
{
    /// <summary>
    /// Страница управления категориями платежей
    /// Предоставляет интерфейс для просмотра, добавления, редактирования и удаления категорий
    /// в административной панели системы
    /// </summary>
    public partial class CategoryTabPage : Page
    {
        /// <summary>
        /// Конструктор страницы управления категориями
        /// Инициализирует компоненты и подписывается на событие загрузки страницы
        /// </summary>
        public CategoryTabPage()
        {
            InitializeComponent();
            this.Loaded += Page_Loaded; // Подписка на событие загрузки для инициализации данных
        }

        /// <summary>
        /// Обработчик события загрузки страницы
        /// Выполняет первоначальную загрузку данных в DataGrid при отображении страницы
        /// </summary>
        /// <param name="sender">Источник события (страница)</param>
        /// <param name="e">Аргументы события загрузки</param>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshData(); // Инициализация данных при загрузке страницы
        }

        /// <summary>
        /// Обновляет данные в DataGrid категорий
        /// Перезагружает данные из базы данных и обновляет привязку DataGrid
        /// </summary>
        private void RefreshData()
        {
            // Принудительная перезагрузка всех отслеживаемых сущностей из базы данных
            DbContextHelper.GetContext().ChangeTracker.Entries().ToList().ForEach(x => x.Reload());

            // Установка источника данных для DataGrid с загрузкой всех категорий
            DataGridCategory.ItemsSource = DbContextHelper.GetContext().Category.ToList();
        }

        /// <summary>
        /// Обработчик события добавления новой категории
        /// Навигация на страницу создания новой категории с пустым объектом
        /// </summary>
        /// <param name="sender">Источник события (кнопка добавления)</param>
        /// <param name="e">Аргументы события нажатия кнопки</param>
        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddCategoryPage(null)); // Передача null для создания новой категории
        }

        /// <summary>
        /// Обработчик события редактирования категории
        /// Навигация на страницу редактирования выбранной категории
        /// </summary>
        /// <param name="sender">Источник события (кнопка редактирования в DataGrid)</param>
        /// <param name="e">Аргументы события нажатия кнопки</param>
        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            // Получение объекта категории из DataContext кнопки
            var category = (sender as Button).DataContext as Category;

            // Навигация на страницу редактирования с передачей выбранной категории
            NavigationService?.Navigate(new AddCategoryPage(category));
        }

        /// <summary>
        /// Обработчик события удаления категорий
        /// Выполняет удаление выбранных категорий с подтверждением операции
        /// </summary>
        /// <param name="sender">Источник события (кнопка удаления)</param>
        /// <param name="e">Аргументы события нажатия кнопки</param>
        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            // Преобразование выбранных элементов в список категорий
            var categoryForRemoving = DataGridCategory.SelectedItems.Cast<Category>().ToList();

            // Проверка наличия выбранных элементов для удаления
            if (categoryForRemoving.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы одну запись для удаления.");
                return; // Прерывание выполнения при отсутствии выбранных элементов
            }

            // Запрос подтверждения удаления с указанием количества элементов
            if (MessageBox.Show($"Вы точно хотите удалить {categoryForRemoving.Count} элемент(ов)?",
                                "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    // Удаление каждой выбранной категории из контекста данных
                    foreach (var item in categoryForRemoving)
                        DbContextHelper.GetContext().Category.Remove(item);

                    // Сохранение изменений в базе данных
                    DbContextHelper.GetContext().SaveChanges();
                    MessageBox.Show("Данные успешно удалены!");

                    // Обновление данных в DataGrid после удаления
                    RefreshData();
                }
                catch (Exception ex)
                {
                    // Обработка исключений при работе с базой данных
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }
    }
}