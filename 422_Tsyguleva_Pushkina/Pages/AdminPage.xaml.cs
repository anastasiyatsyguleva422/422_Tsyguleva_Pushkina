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
    /// Главная страница администратора системы
    /// Предоставляет навигацию по основным разделам администрирования:
    /// управление пользователями, категориями, платежами и аналитика
    /// </summary>
    public partial class AdminPage : Page
    {
        /// <summary>
        /// Конструктор главной страницы администратора
        /// Инициализирует компоненты интерфейса панели управления
        /// </summary>
        public AdminPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обработчик навигации на вкладку управления пользователями
        /// Переход на страницу работы с пользовательскими учетными записями
        /// </summary>
        /// <param name="sender">Источник события (кнопка навигации)</param>
        /// <param name="e">Аргументы события нажатия кнопки</param>
        private void BtnTab1_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new UsersTabPage());
        }

        /// <summary>
        /// Обработчик навигации на вкладку управления категориями
        /// Переход на страницу работы с категориями платежей
        /// </summary>
        /// <param name="sender">Источник события (кнопка навигации)</param>
        /// <param name="e">Аргументы события нажатия кнопки</param>
        private void BtnTab2_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new CategoryTabPage());
        }

        /// <summary>
        /// Обработчик навигации на вкладку управления платежами
        /// Переход на страницу работы с финансовыми операциями
        /// </summary>
        /// <param name="sender">Источник события (кнопка навигации)</param>
        /// <param name="e">Аргументы события нажатия кнопки</param>
        private void BtnTab3_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new PaymentTabPage());
        }

        /// <summary>
        /// Обработчик навигации на вкладку аналитики и диаграмм
        /// Переход на страницу визуализации данных и отчетности
        /// </summary>
        /// <param name="sender">Источник события (кнопка навигации)</param>
        /// <param name="e">Аргументы события нажатия кнопки</param>
        private void BtnTab4_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new DiagrammPage());
        }
    }
}