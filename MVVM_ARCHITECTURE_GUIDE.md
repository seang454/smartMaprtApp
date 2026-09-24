# 📘 Complete Guide: Modern WPF, MVVM & Clean Architecture

This document summarizes all key architectural concepts, comparisons, and rules used in **SmallMartApp** for your **OOAD (Object-Oriented Analysis & Design)** coursework.

---

## 1. 🏛️ The MVVM Triad: Model vs. View vs. ViewModel

```text
┌─────────────────┐       ┌───────────────────────────┐       ┌────────────────────────┐
│   MODEL (C#)    │       │      VIEWMODEL (C#)       │       │      VIEW (XAML)       │
│                 │       │                           │       │                        │
│ • Raw Data      │◄─────►│ • Prepares data for View  │◄─────►│ • What user sees       │
│ • Database row  │       │ • ObservableCollection<T> │       │ • DataGrid Table       │
│ • Product.cs    │       │ • Button Click Commands   │       │ • ProductListView.xaml │
│ • Category.cs   │       │ • ProductListViewModel.cs │       │                        │
└─────────────────┘       └───────────────────────────┘       └────────────────────────┘
        ▲
        │ Saves / Loads via Services
        ▼
┌─────────────────┐
│   SQL Server    │
│  (SmallMartDb)  │
└─────────────────┘
```

### The Big Difference

| Layer | Does it interact with UI? | What is inside it? | Example File |
| :--- | :--- | :--- | :--- |
| **Model** | ❌ **NO.** Zero knowledge of buttons or screens. | Just **raw properties** representing database tables. | `Product.cs`, `Category.cs` |
| **View** | 👁️ **Visual Only.** Renders the screen. | **Layout, Grids, Margins, Colors, Fonts.** | `ProductListView.xaml` |
| **ViewModel** | ✅ **YES! 100% of UI logic lives here.** | **Button commands, math, search text, lists, validations.** | `ProductListViewModel.cs` |

---

### Why the MODEL does NOT have UI logic
```csharp
// Product.cs (MODEL) - In Core/Features/Products/
// It is just a dumb data container! No logic!
public class Product : BaseEntity
{
    public string Barcode { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal SellPrice { get; set; }
    public int StockQuantity { get; set; }
}
```
* Does `Product.cs` know when the user clicks a button? **No.**
* Does `Product.cs` know what text is inside a search box? **No.**

---

### Where the UI Logic ACTUALLY lives: The VIEWMODEL
```csharp
// ProductListViewModel.cs (VIEWMODEL) - In UI/ViewModels/
// THIS is where all the UI interaction logic lives!
public partial class ProductListViewModel : ViewModelBase
{
    // 1. Interacts with the search box on screen:
    [ObservableProperty]
    private string _searchText = "";

    // 2. Interacts with the "Save" button on screen:
    [RelayCommand]
    public async Task AddProductAsync()
    {
        // UI validation logic:
        if (string.IsNullOrEmpty(NewBarcode))
        {
            StatusMessage = "Please enter barcode!";
            return;
        }

        // Call the service to save to SQL Server:
        await _productService.AddOrUpdateAsync(...);
    }
}
```

---

## 2. 📄 Why `.xaml` has a `.xaml.cs` File

```text
       CategoryView.xaml                          CategoryView.xaml.cs
   (Visual Layout in XAML)                     (C# Anchor Code-Behind)
              │                                           │
              └─────────────────────┬─────────────────────┘
                                    │ Combined by Compiler
                                    ▼
                  ┌───────────────────────────────────┐
                  │      class CategoryView           │
                  │   (Single compiled .NET class)    │
                  └───────────────────────────────────┘
```

1. **Computers cannot run raw XAML text directly:** Windows and the .NET runtime only execute compiled C# byte code.
2. **`partial class` keyword:** Tells the C# compiler that `CategoryView.xaml` and `CategoryView.xaml.cs` are **two halves of the exact same class**.
3. **What `InitializeComponent()` does:**
   ```csharp
   public CategoryView()
   {
       InitializeComponent(); // Tells Windows: "Read CategoryView.xaml and draw it on screen right now!"
   }
   ```
4. **Why `.xaml.cs` is almost empty in MVVM:** In clean MVVM, you do NOT write button clicks or database queries in the code-behind. You put them in the **ViewModel**!

---

## 3. 🔗 How View and ViewModel Interact (No UI Names Needed!)

In older technologies (like Windows Forms), you had to write `txtBarcode.Text` by control name.  
In modern WPF, **you don't need UI names.** You use:

### A. Data Binding (For Data & Input)
* **ViewModel:** `[ObservableProperty] private string _barcodeInput = "";`
* **XAML:** `<TextBox Text="{Binding BarcodeInput, UpdateSourceTrigger=PropertyChanged}" />`
* **Result:** Whatever the cashier types on screen automatically flows into `BarcodeInput` in C#.

### B. Commands (For Button Clicks via `[RelayCommand]`)
In standard WPF, a `<Button>` only understands an `ICommand`. In the past, developers had to write 30+ lines of boilerplate per button.  
The **MVVM Community Toolkit** gives you **`[RelayCommand]`**, which automatically generates the command property behind the scenes.

#### The Naming Convention:
* If method is `public void CompleteSale()` $\rightarrow$ Generates `CompleteSaleCommand`.
* If method ends with `Async` like `public async Task NavigateToDashboardAsync()` $\rightarrow$ Strips `Async` and appends `Command`: **`NavigateToDashboardCommand`**.

#### The 4 Types of `[RelayCommand]`:
1. **Simple Synchronous (No params):**
   ```csharp
   [RelayCommand]
   private void NavigateToPos() { CurrentView = _posVm; }
   // Generates: IRelayCommand NavigateToPosCommand
   ```
2. **Asynchronous (`async Task`):**
   ```csharp
   [RelayCommand]
   private async Task LoadProductsAsync() { ... }
   // Generates: IAsyncRelayCommand LoadProductsCommand (includes built-in .IsRunning for spinners!)
   ```
3. **With Parameter (`T`):**
   ```csharp
   [RelayCommand]
   private void DeleteProduct(Product product) { ... }
   // Generates: IRelayCommand<Product> DeleteProductCommand
   ```
4. **Asynchronous with Parameter (`async Task` + `T`):**
   ```csharp
   [RelayCommand]
   private async Task SaveCustomerAsync(Customer customer) { ... }
   // Generates: IAsyncRelayCommand<Customer> SaveCustomerCommand
   ```

#### 2 Extra Superpowers:
* **`CanExecute` (Auto-disable button):** `[RelayCommand(CanExecute = nameof(CanCheckout))]` — Button automatically greys out when conditions are not met!
* **`CancellationToken`:** Pass a token to allow users to cancel long-running operations.

---

## 4. 🔀 What is `DataTemplate` and How Does View Switching Work?

### A. What is a `DataTemplate`?
A **`DataTemplate`** is a visual blueprint that tells WPF:
> *"When you receive this C# object, **THIS** is how you should draw it on screen."*

Without a `DataTemplate`, WPF has no idea how to draw a C# object, so it falls back to calling `.ToString()`, which prints ugly raw text on screen:  
`SmallMartApp.UI.ViewModels.DashboardViewModel`

### B. What is `DataTemplate` used for?
1. **Cards & Lists (`ListBox`, `ItemsControl`):** Turning rows of data into Amazon-style visual cards with pictures, badges, and action buttons.
2. **Custom Dropdowns (`ComboBox`):** Displaying icons, colors, or multi-column data in dropdowns.
3. **View Switching (Screen Navigation):** Mapping a ViewModel class to a full-screen View UserControl.

---

### C. The 5-Step Chain Reaction: How Clicking the Sidebar Shows the Screen

```
[1. User clicks Button]
       │
       ▼
[2. MainViewModel runs command: CurrentView = _dashboardVm]
       │
       ▼ (Fires PropertyChanged notification)
[3. <ContentControl Content="{Binding CurrentView}"/> hears change]
       │
       ▼ (Searches resources for a DataTemplate recipe)
[4. DataTemplate matches: vm:DashboardViewModel -> views:DashboardView]
       │
       ▼
[5. WPF instantiates and displays DashboardView on screen!]
```

> **Key Rule of MVVM:** `DashboardViewModel` **never** calls or references `DashboardView`!  
> The ViewModel is completely decoupled from the UI. WPF's data binding and `DataTemplate` handle the visual rendering automatically.

---

### D. Resource Scopes: Where to Declare `DataTemplate`s?

| Scope | Location | Visibility / Accessibility |
| :--- | :--- | :--- |
| **Control Scope** | `<UserControl.Resources>` or `<Grid.Resources>` | Only accessible inside that single control. |
| **Window Scope** | `<Window.Resources>` in `MainWindow.xaml` | Accessible anywhere inside that specific Window. |
| **Application Scope (Global)** | `<Application.Resources>` in `App.xaml` | **Accessible globally** across all windows, popups, and dialogs in the entire app. |

## 5. 🗄️ Persistence vs. Service

| Concept | Responsibility | Question it Answers | Project Example |
| :--- | :--- | :--- | :--- |
| **Persistence** | **Data storage & retrieval** | *"Where and how is data saved?"* | `SmallMartDbContext.cs`<br>(EF Core, SQL Server, Tables, Foreign Keys) |
| **Service** | **Business rules & actions** | *"What decisions and steps must happen?"* | `SalesService.cs`, `ProductService.cs`<br>(Stock deduction, validation, receipt printing) |

---

## 6. 🧠 3-Second Memory Rules & Everyday Analogies

### The Restaurant Analogy:
* **View (The Plate & Menu):** The visual presentation the customer looks at.
* **ViewModel (The Waiter):** Takes your order, checks with the kitchen, calculates the bill, and brings the plate.
* **Model / Persistence (The Kitchen):** The ingredients and safe storage where raw food (database records) is kept.

### The 3-Second Decision Rule:
* *"Is this about colors, margins, fonts, screen layout, or styling?"*  
  $\rightarrow$ Put it in the **VIEW** (`.xaml`).
* *"Is this about math, button clicks, search filters, lists, or screen behavior?"*  
  $\rightarrow$ Put it in the **VIEWMODEL** (`.cs`).
* *"Is this about table columns and raw database fields?"*  
  $\rightarrow$ Put it in the **MODEL** (`.cs`).
* *"Is this about database queries (`SELECT`, `INSERT`, `UPDATE`)?"*  
  $\rightarrow$ Put it in **PERSISTENCE** (`DbContext`).
* *"Is this about business rules (stock deduction, change calculation, validations)?"*  
  $\rightarrow$ Put it in a **SERVICE** (`SalesService.cs`).