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
    /// Страница аутентификации пользователей в системе
    /// Обеспечивает безопасный вход с проверкой учетных данных, защитой от перебора и CAPTCHA-верификацией
    /// </summary>
    public partial class AuthPage : Page
    {
        // Счетчик неудачных попыток входа для активации CAPTCHA
        private int failedAttempts = 0;

        // Текущий авторизуемый пользователь
        private User currentUser;

        /// <summary>
        /// Конструктор страницы аутентификации
        /// Инициализирует компоненты интерфейса входа в систему
        /// </summary>
        public AuthPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Генерирует SHA1 хеш для безопасного хранения паролей
        /// Преобразует пароль в криптографический хеш для сравнения с хранимым значением
        /// </summary>
        /// <param name="password">Пароль в открытом виде</param>
        /// <returns>Хешированное представление пароля в HEX-формате</returns>
        public static string GetHash(String password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        /// <summary>
        /// Обработчик события входа в систему
        /// Выполняет аутентификацию пользователя с валидацией учетных данных и защитой от брутфорса
        /// </summary>
        /// <param name="sender">Источник события (кнопка входа)</param>
        /// <param name="e">Аргументы события нажатия кнопки</param>
        private void ButtonEnter_OnClick(object sender, RoutedEventArgs e)
        {
            // Проверка заполнения обязательных полей
            if (string.IsNullOrEmpty(TextBoxLogin.Text) || string.IsNullOrEmpty(PasswordBox.Password))
            {
                MessageBox.Show("Введите логин или пароль");
                return;
            }

            // Хеширование введенного пароля для безопасного сравнения
            string hashedPassword = GetHash(PasswordBox.Password);

            using (var db = new Tsyguleva_Pushkina_DB_PaymentEntities())
            {
                // Поиск пользователя в базе данных по логину и хешу пароля
                var user = db.User
                    .AsNoTracking() // Отключение отслеживания изменений для оптимизации
                    .FirstOrDefault(u => u.Login == TextBoxLogin.Text && u.Password == hashedPassword);

                if (user == null)
                {
                    MessageBox.Show("Пользователь с такими данными не найден!");
                    failedAttempts++;

                    // Активация CAPTCHA после 3 неудачных попыток
                    if (failedAttempts >= 3)
                    {
                        if (captcha.Visibility != Visibility.Visible)
                        {
                            CaptchaSwitch(); // Переключение в режим CAPTCHA
                        }
                        CaptchaChange(); // Генерация новой CAPTCHA
                    }
                    return;
                }
                else
                {
                    MessageBox.Show("Пользователь успешно найден!");

                    // Навигация в зависимости от роли пользователя
                    switch (user.Role)
                    {
                        case "User":
                            NavigationService?.Navigate(new Pages.UserPage());
                            break;
                        case "Admin":
                            NavigationService?.Navigate(new Pages.AdminPage());
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Обработчик перехода на страницу регистрации
        /// Перенаправляет пользователя на форму создания новой учетной записи
        /// </summary>
        /// <param name="sender">Источник события (кнопка регистрации)</param>
        /// <param name="e">Аргументы события нажатия кнопки</param>
        private void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegPage());
        }

        /// <summary>
        /// Обработчик перехода на страницу смены пароля
        /// Перенаправляет пользователя на форму восстановления доступа
        /// </summary>
        /// <param name="sender">Источник события (кнопка смены пароля)</param>
        /// <param name="e">Аргументы события нажатия кнопки</param>
        private void ButtonChangePassword_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ChangePassPage());
        }

        /// <summary>
        /// Обработчик клика по подсказке логина
        /// Устанавливает фокус ввода на поле логина при клике на текстовую подсказку
        /// </summary>
        /// <param name="sender">Источник события (текстовая подсказка)</param>
        /// <param name="e">Аргументы события клика мышью</param>
        private void txtHintLogin_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBoxLogin.Focus();
        }

        /// <summary>
        /// Обработчик клика по подсказке пароля
        /// Устанавливает фокус ввода на поле пароля при клике на текстовую подсказку
        /// </summary>
        /// <param name="sender">Источник события (текстовая подсказка)</param>
        /// <param name="e">Аргументы события клика мышью</param>
        private void txtHintPass_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PasswordBox.Focus();
        }

        /// <summary>
        /// Переключает режим отображения CAPTCHA
        /// Управляет видимостью элементов интерфейса при активации/деактивации CAPTCHA
        /// </summary>
        public void CaptchaSwitch()
        {
            switch (captcha.Visibility)
            {
                case Visibility.Visible:
                    // Сброс полей и возврат к нормальному режиму входа
                    TextBoxLogin.Clear();
                    PasswordBox.Clear();
                    captcha.Visibility = Visibility.Hidden;
                    captchaInput.Visibility = Visibility.Hidden;
                    submitCaptcha.Visibility = Visibility.Hidden;
                    labelLogin.Visibility = Visibility.Visible;
                    labelPass.Visibility = Visibility.Visible;
                    TextBoxLogin.Visibility = Visibility.Visible;
                    PasswordBox.Visibility = Visibility.Visible;
                    ButtonChangePassword.Visibility = Visibility.Visible;
                    ButtonEnter.Visibility = Visibility.Visible;
                    ButtonReg.Visibility = Visibility.Visible;
                    return;
                case Visibility.Hidden:
                    // Активация режима CAPTCHA с скрытием основных элементов входа
                    captcha.Visibility = Visibility.Visible;
                    captchaInput.Visibility = Visibility.Visible;
                    submitCaptcha.Visibility = Visibility.Visible;
                    labelLogin.Visibility = Visibility.Hidden;
                    labelPass.Visibility = Visibility.Hidden;
                    TextBoxLogin.Visibility = Visibility.Hidden;
                    PasswordBox.Visibility = Visibility.Hidden;
                    ButtonChangePassword.Visibility = Visibility.Hidden;
                    ButtonEnter.Visibility = Visibility.Hidden;
                    ButtonReg.Visibility = Visibility.Hidden;
                    return;
            }
        }

        /// <summary>
        /// Генерирует новую случайную CAPTCHA
        /// Создает строку из 6 случайных символов (буквы и цифры) для проверки пользователя
        /// </summary>
        public void CaptchaChange()
        {
            // Набор допустимых символов для CAPTCHA
            String allowchar = " ";
            allowchar = "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z";
            allowchar += "a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,y,z";
            allowchar += "1,2,3,4,5,6,7,8,9,0";

            char[] a = { ',' };
            String[] ar = allowchar.Split(a);
            String pwd = "";
            string temp = "";
            Random r = new Random();

            // Генерация 6 случайных символов
            for (int i = 0; i < 6; i++)
            {
                temp = ar[(r.Next(0, ar.Length))];
                pwd += temp;
            }
            captcha.Text = pwd;
        }

        /// <summary>
        /// Обработчик проверки введенной CAPTCHA
        /// Сравнивает пользовательский ввод с сгенерированным кодом CAPTCHA
        /// </summary>
        /// <param name="sender">Источник события (кнопка проверки CAPTCHA)</param>
        /// <param name="e">Аргументы события нажатия кнопки</param>
        private void submitCaptcha_Click(object sender, RoutedEventArgs e)
        {
            if (captchaInput.Text != captcha.Text)
            {
                MessageBox.Show("Неверно введена капча", "Ошибка");
                CaptchaChange(); // Генерация новой CAPTCHA при ошибке
            }
            else
            {
                MessageBox.Show("Капча введена успешно, можете продолжить авторизацию", "Успех");
                CaptchaSwitch(); // Возврат к нормальному режиму
                failedAttempts = 0; // Сброс счетчика неудачных попыток
            }
        }

        /// <summary>
        /// Обработчик запрета операций копирования/вставки/вырезания
        /// Запрещает использование буфера обмена для повышения безопасности ввода
        /// </summary>
        /// <param name="sender">Источник события (текстовое поле)</param>
        /// <param name="e">Аргументы события выполнения команды</param>
        private void textBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true; // Блокировка команды
            }
        }

        /// <summary>
        /// Обработчик изменения текста в поле пароля
        /// Может быть использован для реализации дополнительной валидации
        /// </summary>
        /// <param name="sender">Источник события (поле пароля)</param>
        /// <param name="e">Аргументы события изменения текста</param>
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Обработчик изменения пароля
            // Можно добавить логику, если нужно
        }

        /// <summary>
        /// Обработчик изменения текста в поле логина
        /// Может быть использован для реализации дополнительной валидации
        /// </summary>
        /// <param name="sender">Источник события (поле логина)</param>
        /// <param name="e">Аргументы события изменения текста</param>
        private void TextBoxLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Обработчик изменения логина
            // Можно добавить логику, если нужно
        }
    }
}