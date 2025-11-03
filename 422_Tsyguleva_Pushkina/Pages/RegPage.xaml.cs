using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    /// Страница регистрации новых пользователей в системе
    /// Предоставляет функциональность создания учетных записей с валидацией данных и безопасным хранением паролей
    /// </summary>
    public partial class RegPage : Page
    {
        /// <summary>
        /// Конструктор страницы регистрации
        /// Инициализирует компоненты и устанавливает значение по умолчанию для выбора роли
        /// </summary>
        public RegPage()
        {
            InitializeComponent();
            comboBxRole.SelectedIndex = 0; // Установка начального значения выпадающего списка ролей
        }

        /// <summary>
        /// Генерирует SHA1 хеш для безопасного хранения паролей в базе данных
        /// Преобразует пароль в криптографический хеш для защиты конфиденциальных данных
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
        /// Обработчик события регистрации нового пользователя
        /// Выполняет комплексную валидацию данных и создание новой учетной записи
        /// </summary>
        /// <param name="sender">Источник события (кнопка регистрации)</param>
        /// <param name="e">Аргументы события нажатия кнопки</param>
        private void regButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка заполнения всех обязательных полей формы
            if (string.IsNullOrEmpty(txtbxLog.Text) ||
                string.IsNullOrEmpty(txtbxFIO.Text) ||
                string.IsNullOrEmpty(passBxFrst.Password) ||
                string.IsNullOrEmpty(passBxScnd.Password))
            {
                MessageBox.Show("Заполните все поля!");
                return; // Прерывание выполнения при незаполненных полях
            }

            // Проверка уникальности логина в системе
            using (var db = new Tsyguleva_Pushkina_DB_PaymentEntities())
            {
                var user = db.User
                    .AsNoTracking() // Отключение отслеживания для оптимизации производительности
                    .FirstOrDefault(u => u.Login == txtbxLog.Text);

                if (user != null)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!");
                    return; // Прерывание выполнения при дублировании логина
                }
            }

            // Проверка минимальной длины пароля
            if (passBxFrst.Password.Length >= 6)
            {
                bool en = true;  // Флаг использования только английских символов
                bool number = false; // Флаг наличия хотя бы одной цифры

                // Анализ каждого символа пароля на соответствие требованиям сложности
                for (int i = 0; i < passBxFrst.Password.Length; i++)
                {
                    if (passBxFrst.Password[i] >= '0' && passBxFrst.Password[i] <= '9')
                        number = true; // Установка флага при обнаружении цифры
                    else if (!((passBxFrst.Password[i] >= 'A' && passBxFrst.Password[i] <= 'Z') ||
                              (passBxFrst.Password[i] >= 'a' && passBxFrst.Password[i] <= 'z')))
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
                    return; // Прерывание при отсутствии цифр в пароле
                }
            }
            else
            {
                MessageBox.Show("Пароль слишком короткий, должно быть минимум 6 символов!");
                return; // Прерывание при недостаточной длине пароля
            }

            // Проверка совпадения пароля и подтверждения пароля
            if (passBxFrst.Password != passBxScnd.Password)
            {
                MessageBox.Show("Пароли не совпадают!");
                return; // Прерывание при несовпадении паролей
            }

            // Создание и сохранение нового пользователя в базе данных
            using (var db = new Tsyguleva_Pushkina_DB_PaymentEntities())
            {
                // Создание объекта пользователя с хешированным паролем
                User userObject = new User
                {
                    FIO = txtbxFIO.Text,
                    Login = txtbxLog.Text,
                    Password = GetHash(passBxFrst.Password), // Безопасное хранение пароля
                    Role = comboBxRole.Text
                };

                // Добавление пользователя в контекст данных и сохранение изменений
                db.User.Add(userObject);
                db.SaveChanges();

                // Уведомление об успешной регистрации и очистка формы
                MessageBox.Show("Пользователь успешно зарегистрирован!");
                txtbxLog.Clear();
                passBxFrst.Clear();
                passBxScnd.Clear();
                comboBxRole.SelectedIndex = 0;
                txtbxFIO.Clear();
            }
        }

        /// <summary>
        /// Обработчик клика по подсказке логина
        /// Устанавливает фокус ввода на поле логина при клике на текстовую подсказку
        /// </summary>
        /// <param name="sender">Источник события (текстовая подсказка)</param>
        /// <param name="e">Аргументы события клика мышью</param>
        private void lblLogHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            txtbxLog.Focus();
        }

        /// <summary>
        /// Обработчик клика по подсказке первого пароля
        /// Устанавливает фокус ввода на поле первого пароля при клике на текстовую подсказку
        /// </summary>
        /// <param name="sender">Источник события (текстовая подсказка)</param>
        /// <param name="e">Аргументы события клика мышью</param>
        private void lblPassHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            passBxFrst.Focus();
        }

        /// <summary>
        /// Обработчик клика по подсказке второго пароля
        /// Устанавливает фокус ввода на поле подтверждения пароля при клике на текстовую подсказку
        /// </summary>
        /// <param name="sender">Источник события (текстовая подсказка)</param>
        /// <param name="e">Аргументы события клика мышью</param>
        private void lblPassSecHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            passBxScnd.Focus();
        }

        /// <summary>
        /// Обработчик клика по подсказке ФИО
        /// Устанавливает фокус ввода на поле ФИО при клике на текстовую подсказку
        /// </summary>
        /// <param name="sender">Источник события (текстовая подсказка)</param>
        /// <param name="e">Аргументы события клика мышью</param>
        private void lblFioHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            txtbxFIO.Focus();
        }

        /// <summary>
        /// Обработчик изменения текста в поле логина
        /// Управляет видимостью текстовой подсказки в зависимости от наличия текста
        /// </summary>
        /// <param name="sender">Источник события (поле логина)</param>
        /// <param name="e">Аргументы события изменения текста</param>
        private void txtbxLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblLogHitn.Visibility = Visibility.Visible;
            if (txtbxLog.Text.Length > 0)
            {
                lblLogHitn.Visibility = Visibility.Hidden; // Скрытие подсказки при наличии текста
            }
        }

        /// <summary>
        /// Обработчик изменения текста в поле ФИО
        /// Управляет видимостью текстовой подсказки в зависимости от наличия текста
        /// </summary>
        /// <param name="sender">Источник события (поле ФИО)</param>
        /// <param name="e">Аргументы события изменения текста</param>
        private void txtbxFIO_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblFioHitn.Visibility = Visibility.Visible;
            if (txtbxFIO.Text.Length > 0)
            {
                lblFioHitn.Visibility = Visibility.Hidden; // Скрытие подсказки при наличии текста
            }
        }

        /// <summary>
        /// Обработчик изменения текста в поле первого пароля
        /// Управляет видимостью текстовой подсказки в зависимости от наличия текста
        /// </summary>
        /// <param name="sender">Источник события (поле первого пароля)</param>
        /// <param name="e">Аргументы события изменения пароля</param>
        private void passBxFrst_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblPassHitn.Visibility = Visibility.Visible;
            if (passBxFrst.Password.Length > 0)
            {
                lblPassHitn.Visibility = Visibility.Hidden; // Скрытие подсказки при наличии текста
            }
        }

        /// <summary>
        /// Обработчик изменения текста в поле второго пароля
        /// Управляет видимостью текстовой подсказки в зависимости от наличия текста
        /// </summary>
        /// <param name="sender">Источник события (поле второго пароля)</param>
        /// <param name="e">Аргументы события изменения пароля</param>
        private void passBxScnd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblPassSecHitn.Visibility = Visibility.Visible;
            if (passBxScnd.Password.Length > 0)
            {
                lblPassSecHitn.Visibility = Visibility.Hidden; // Скрытие подсказки при наличии текста
            }
        }
    }
}