using CommunityToolkit.Mvvm.ComponentModel;

namespace SmallMartApp.UI.ViewModels;

public partial class CartItemViewModel : ObservableObject
{
    public int ProductId { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Subtotal))]
    private int _quantity = 1;

    public decimal Subtotal => Quantity * UnitPrice;
}
