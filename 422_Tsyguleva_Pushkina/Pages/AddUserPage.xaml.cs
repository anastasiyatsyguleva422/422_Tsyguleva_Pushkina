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
    /// Страница для добавления и редактирования пользователей
    /// Предоставляет интерфейс для управления данными пользователей системы
    /// </summary>
    public partial class AddUserPage : Page
    {
        // Текущий пользователь для работы (новый или редактируемый)
        private User _currentUser = new User();

        /// <summary>
        /// Конструктор для создания или редактирования пользователя
        /// </summary>
        /// <param name="selectedUser">Существующий пользователь для редактирования (null для создания нового)</param>
        public AddUserPage(User selectedUser)
        {
            InitializeComponent();

            // Инициализация текущего пользователя
            if (selectedUser != null)
                _currentUser = selectedUser;

            // Привязка данных к элементам управления
            DataContext = _currentUser;
        }

        /// <summary>
        /// Обработчик события сохранения пользователя
        /// Выполняет валидацию данных и сохраняет пользователя в базу данных
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            // StringBuilder для накопления ошибок валидации
            StringBuilder errors = new StringBuilder();

            // Валидация обязательных полей
            if (string.IsNullOrWhiteSpace(_currentUser.Login))
                errors.AppendLine("Укажите логин!");
            if (string.IsNullOrWhiteSpace(_currentUser.Password))
                errors.AppendLine("Укажите пароль!");

            // Валидация выбора роли
            if ((_currentUser.Role == null) || (cmbRole.Text == ""))
                errors.AppendLine("Выберите роль!");
            else
                _currentUser.Role = cmbRole.Text; // Установка значения роли из ComboBox

            if (string.IsNullOrWhiteSpace(_currentUser.FIO))
                errors.AppendLine("Укажите ФИО");

            // Проверка наличия ошибок валидации
            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return; // Прерывание выполнения при наличии ошибок
            }

            // Определение типа операции: добавление нового пользователя
            if (_currentUser.ID == 0)
                DbContextHelper.GetContext().User.Add(_currentUser);

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
        /// Сбрасывает значения всех полей ввода до состояния по умолчанию
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события</param>
        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            // Очистка всех текстовых полей формы
            TBLogin.Text = "";
            TBPass.Text = "";
            cmbRole.Items.Clear(); // Очистка элементов выпадающего списка ролей
            TBFio.Text = "";
            TBPhoto.Text = "";
        }
    }
}