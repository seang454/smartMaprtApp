using System.Windows;
using SmallMartApp.UI.ViewModels;

namespace SmallMartApp.UI;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel mainVm)
    {
        InitializeComponent();
        DataContext = mainVm;
    }
}
