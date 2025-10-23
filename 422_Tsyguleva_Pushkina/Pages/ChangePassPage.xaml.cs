using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace _422_Tsyguleva_Pushkina.Pages
{
    public partial class ChangePassPage : Page
    {
        public ChangePassPage()
        {
            InitializeComponent();
        }

        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        private void ButtonEnter_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentPasswordBox.Password) ||
                string.IsNullOrEmpty(NewPasswordBox.Password) ||
                string.IsNullOrEmpty(ConfirmPasswordBox.Password) ||
                string.IsNullOrEmpty(TbLogin.Text))
            {
                MessageBox.Show("Все поля обязательны к заполнению!");
                return;
            }

            if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Пароли не совпадают!");
                return;
            }

            string hashedPass = GetHash(CurrentPasswordBox.Password);

            try
            {
                using (var context = DbContextHelper.GetContext())
                {
                    var user = context.User
                        .FirstOrDefault(u => u.Login == TbLogin.Text && u.Password == hashedPass);

                    if (user == null)
                    {
                        MessageBox.Show("Текущий пароль/Логин неверный!");
                        return;
                    }

                    if (NewPasswordBox.Password.Length < 6)
                    {
                        MessageBox.Show("Пароль слишком короткий, должно быть минимум 6 символов!");
                        return;
                    }

                    bool en = true;
                    bool number = false;

                    foreach (char c in NewPasswordBox.Password)
                    {
                        if (c >= '0' && c <= '9')
                            number = true;
                        else if (!((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')))
                            en = false;
                    }

                    if (!en)
                    {
                        MessageBox.Show("Используйте только английскую раскладку!");
                        return;
                    }
                    else if (!number)
                    {
                        MessageBox.Show("Добавьте хотя бы одну цифру!");
                        return;
                    }

                    user.Password = GetHash(NewPasswordBox.Password);

                    if (context.SaveChanges() > 0)
                    {
                        MessageBox.Show("Пароль успешно изменен!");
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
                MessageBox.Show($"Ошибка при изменении пароля: {ex.Message}");
            }
        }
    }
}