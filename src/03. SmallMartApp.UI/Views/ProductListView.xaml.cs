using System.Windows;
using System.Windows.Controls;
using SmallMartApp.UI.ViewModels;

namespace SmallMartApp.UI.Views;

public partial class ProductListView : UserControl
{
    public ProductListView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProductListViewModel vm)
        {
            await vm.LoadProductsCommand.ExecuteAsync(null);
        }
    }
}
