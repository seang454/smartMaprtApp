using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmallMartApp.Core.Features.Products;

namespace SmallMartApp.UI.ViewModels;

public partial class CategoryViewModel : ViewModelBase
{
    private readonly ICategoryService _categoryService;

    public ObservableCollection<Category> Categories { get; } = new();

    [ObservableProperty]
    private string _newCategoryName = string.Empty;

    [ObservableProperty]
    private string _newDescription = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public CategoryViewModel(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [RelayCommand]
    public async Task LoadCategoriesAsync()
    {
        Categories.Clear();
        var items = await _categoryService.GetAllAsync();
        foreach (var item in items) Categories.Add(item);
    }

    [RelayCommand]
    public async Task AddCategoryAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCategoryName))
        {
            StatusMessage = "Category name is required.";
            return;
        }

        var cat = new Category { Name = NewCategoryName.Trim(), Description = NewDescription.Trim() };
        var res = await _categoryService.AddOrUpdateAsync(cat);
        if (res.IsSuccess)
        {
            StatusMessage = $"Added category '{cat.Name}' successfully!";
            NewCategoryName = string.Empty;
            NewDescription = string.Empty;
            await LoadCategoriesAsync();
        }
        else
        {
            StatusMessage = res.Error ?? "Error adding category.";
        }
    }

    [RelayCommand]
    public async Task DeleteCategoryAsync(Category cat)
    {
        if (cat == null) return;
        var res = await _categoryService.DeleteAsync(cat.Id);
        if (res.IsSuccess) Categories.Remove(cat);
    }
}
