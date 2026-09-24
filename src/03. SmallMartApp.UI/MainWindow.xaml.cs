using System.Windows;
using SmallMartApp.UI.ViewModels;

namespace SmallMartApp.UI;

public partial class MainWindow : Window
{
    public MainWindow(PosCheckoutViewModel posVm, ProductListViewModel inventoryVm)
    {
        InitializeComponent();
        PosView.DataContext = posVm;
        InventoryView.DataContext = inventoryVm;
    }
}
