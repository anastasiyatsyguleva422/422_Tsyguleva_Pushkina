using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace _422_Tsyguleva_Pushkina.Pages
{
    public partial class CategoryTabPage : Page
    {
        public CategoryTabPage()
        {
            InitializeComponent();
            this.Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void RefreshData()
        {
            DbContextHelper.GetContext().ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
            DataGridCategory.ItemsSource = DbContextHelper.GetContext().Category.ToList();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddCategoryPage(null));
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            var category = (sender as Button).DataContext as Category;
            NavigationService?.Navigate(new AddCategoryPage(category));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var categoryForRemoving = DataGridCategory.SelectedItems.Cast<Category>().ToList();

            if (categoryForRemoving.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы одну запись для удаления.");
                return;
            }

            if (MessageBox.Show($"Вы точно хотите удалить {categoryForRemoving.Count} элемент(ов)?",
                                "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    foreach (var item in categoryForRemoving)
                        DbContextHelper.GetContext().Category.Remove(item);

                    DbContextHelper.GetContext().SaveChanges();
                    MessageBox.Show("Данные успешно удалены!");
                    RefreshData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }

    }
}