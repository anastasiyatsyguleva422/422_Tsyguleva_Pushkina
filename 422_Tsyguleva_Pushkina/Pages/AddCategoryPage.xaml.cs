using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace _422_Tsyguleva_Pushkina.Pages
{
    public partial class AddCategoryPage : Page
    {
        private Category _currentCategory = new Category();

        public AddCategoryPage()
        {
            InitializeComponent();
            DataContext = _currentCategory;
        }

        public AddCategoryPage(Category selectedCategory)
        {
            InitializeComponent();
            _currentCategory = selectedCategory ?? new Category();
            DataContext = _currentCategory;
        }

        private void ButtonSaveCategory_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_currentCategory.Name))
                errors.AppendLine("Укажите название категории!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            if (_currentCategory.ID == 0)
                DbContextHelper.GetContext().Category.Add(_currentCategory);

            try
            {
                DbContextHelper.GetContext().SaveChanges();
                MessageBox.Show("Данные успешно сохранены!");
                NavigationService.GoBack(); // возвращаемся к таблице
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBCategoryName.Text = "";
        }
    }
}