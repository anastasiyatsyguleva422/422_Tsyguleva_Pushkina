using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace _422_Tsyguleva_Pushkina.Pages
{
    /// <summary>
    /// Страница для добавления и редактирования категорий платежей
    /// Обеспечивает функциональность создания новых категорий и модификации существующих
    /// </summary>
    public partial class AddCategoryPage : Page
    {
        // Текущая категория для работы (новая или редактируемая)
        private Category _currentCategory = new Category();

        /// <summary>
        /// Конструктор для создания новой категории
        /// Инициализирует страницу с пустым объектом Category
        /// </summary>
        public AddCategoryPage()
        {
            InitializeComponent();
            DataContext = _currentCategory; // Привязка данных к элементам интерфейса
        }

        /// <summary>
        /// Конструктор для редактирования существующей категории
        /// </summary>
        /// <param name="selectedCategory">Выбранная категория для редактирования</param>
        public AddCategoryPage(Category selectedCategory)
        {
            InitializeComponent();

            // Инициализация текущей категории (переданная или новая при null)
            _currentCategory = selectedCategory ?? new Category();
            DataContext = _currentCategory; // Установка контекста данных для привязки
        }

        /// <summary>
        /// Обработчик события сохранения категории
        /// Выполняет валидацию данных и сохраняет категорию в базу данных
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void ButtonSaveCategory_Click(object sender, RoutedEventArgs e)
        {
            // StringBuilder для накопления ошибок валидации
            StringBuilder errors = new StringBuilder();

            // Проверка обязательного поля "Название категории"
            if (string.IsNullOrWhiteSpace(_currentCategory.Name))
                errors.AppendLine("Укажите название категории!");

            // Если есть ошибки валидации - показываем и прерываем выполнение
            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            // Определение типа операции: добавление новой категории
            if (_currentCategory.ID == 0)
                DbContextHelper.GetContext().Category.Add(_currentCategory);

            try
            {
                // Сохранение изменений в базе данных
                DbContextHelper.GetContext().SaveChanges();
                MessageBox.Show("Данные успешно сохранены!");

                // Возврат на предыдущую страницу после успешного сохранения
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                // Обработка исключений при работе с базой данных
                MessageBox.Show(ex.Message.ToString());
            }
        }

        /// <summary>
        /// Обработчик события очистки полей формы
        /// Сбрасывает значение поля названия категории
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            // Очистка текстового поля названия категории
            TBCategoryName.Text = "";
        }
    }
}