using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace _422_Tsyguleva_Pushkina.Pages
{
    /// <summary>
    /// Страница смены пароля пользователя
    /// Предоставляет функциональность безопасного изменения пароля с валидацией и хешированием
    /// </summary>
    public partial class ChangePassPage : Page
    {
        /// <summary>
        /// Конструктор страницы смены пароля
        /// Инициализирует компоненты интерфейса для изменения учетных данных
        /// </summary>
        public ChangePassPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Генерирует SHA1 хеш для безопасного хранения паролей
        /// Преобразует пароль в криптографический хеш для безопасного сравнения и хранения
        /// </summary>
        /// <param name="password">Пароль в открытом виде</param>
        /// <returns>Хешированное представление пароля в HEX-формате</returns>
        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        /// <summary>
        /// Обработчик события изменения пароля
        /// Выполняет комплексную валидацию данных и безопасное обновление пароля пользователя
        /// </summary>
        /// <param name="sender">Источник события (кнопка изменения пароля)</param>
        /// <param name="e">Аргументы события нажатия кнопки</param>
        private void ButtonEnter_OnClick(object sender, RoutedEventArgs e)
        {
            // Проверка заполнения всех обязательных полей
            if (string.IsNullOrEmpty(CurrentPasswordBox.Password) ||
                string.IsNullOrEmpty(NewPasswordBox.Password) ||
                string.IsNullOrEmpty(ConfirmPasswordBox.Password) ||
                string.IsNullOrEmpty(TbLogin.Text))
            {
                MessageBox.Show("Все поля обязательны к заполнению!");
                return; // Прерывание выполнения при незаполненных полях
            }

            // Проверка совпадения нового пароля и подтверждения
            if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Пароли не совпадают!");
                return; // Прерывание выполнения при несовпадении паролей
            }

            // Хеширование текущего пароля для проверки аутентичности
            string hashedPass = GetHash(CurrentPasswordBox.Password);

            try
            {
                using (var context = DbContextHelper.GetContext())
                {
                    // Поиск пользователя по логину и хешу текущего пароля
                    var user = context.User
                        .FirstOrDefault(u => u.Login == TbLogin.Text && u.Password == hashedPass);

                    // Проверка существования пользователя и корректности текущего пароля
                    if (user == null)
                    {
                        MessageBox.Show("Текущий пароль/Логин неверный!");
                        return; // Прерывание при неверных учетных данных
                    }

                    // Проверка минимальной длины нового пароля
                    if (NewPasswordBox.Password.Length < 6)
                    {
                        MessageBox.Show("Пароль слишком короткий, должно быть минимум 6 символов!");
                        return; // Прерывание при недостаточной длине пароля
                    }

                    // Валидация сложности пароля
                    bool en = true;  // Флаг использования только английских символов
                    bool number = false; // Флаг наличия цифр

                    // Анализ каждого символа нового пароля
                    foreach (char c in NewPasswordBox.Password)
                    {
                        if (c >= '0' && c <= '9')
                            number = true; // Установка флага при обнаружении цифры
                        else if (!((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')))
                            en = false; // Сброс флага при обнаружении неанглийского символа
                    }

                    // Проверка результатов валидации сложности пароля
                    if (!en)
                    {
                        MessageBox.Show("Используйте только английскую раскладку!");
                        return; // Прерывание при использовании неанглийских символов
                    }
                    else if (!number)
                    {
                        MessageBox.Show("Добавьте хотя бы одну цифру!");
                        return; // Прерывание при отсутствии цифр
                    }

                    // Установка нового хешированного пароля
                    user.Password = GetHash(NewPasswordBox.Password);

                    // Сохранение изменений в базе данных
                    if (context.SaveChanges() > 0)
                    {
                        MessageBox.Show("Пароль успешно изменен!");
                        // Перенаправление на страницу аутентификации после успешного изменения
                        NavigationService?.Navigate(new AuthPage());
                    }
                    else
                    {
                        MessageBox.Show("Не удалось изменить пароль!");
                    }
                }
            }
            catch (Exception ex)
            {
                // Обработка исключений при работе с базой данных
                MessageBox.Show($"Ошибка при изменении пароля: {ex.Message}");
            }
        }
    }
}