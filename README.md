# SmallMartApp - POS & Inventory Management System

A modular, clean Windows Desktop architecture designed for small marts, supermarkets, and retail checkout kiosks. Built with **.NET 8 (WPF)** using **Feature-First / Vertical Slice Architecture**, **MVVM**, and **EF Core SQLite**.

---

## 📁 Project Structure

```text
D:\RUPPClass\year4\OOAD\project\SmallMartApp/
│
├── SmallMartApp.sln                   <-- Solution file (compatible with Visual Studio, VS Code, CLI)
├── SmallMartApp.slnx                  <-- Modern XML solution file (.NET 8/10)
├── Directory.Build.props              <-- Solution-wide build configuration (net8.0, nullable, C# 12)
├── README.md                          <-- Project documentation & architecture guide
│
├── src/
│   │
│   ├── 01. SmallMartApp.Core/          <-- [Class Library] The Brain (Pure C#, zero UI/DB dependencies)
│   │   ├── Common/                    <-- Shared domain classes and helper types
│   │   │   ├── BaseEntity.cs          <-- Base class with common properties (Id, CreatedAt)
│   │   │   └── Result.cs              <-- Functional Result pattern for success/failure handling
│   │   ├── Hardware/                  <-- Hardware abstractions & contracts (Interfaces only)
│   │   │   ├── IBarcodeScanner.cs     <-- Contract for barcode scanning (events & connection)
│   │   │   └── IReceiptPrinter.cs     <-- Contract for printing thermal sales receipts
│   │   └── Features/                  <-- Business features organized by Vertical Slices
│   │       ├── Products/              <-- Inventory domain
│   │       │   ├── Product.cs         <-- Entity: Barcode, Name, CostPrice, SellPrice, StockQuantity
│   │       │   └── IProductService.cs <-- Service interface for product CRUD operations
│   │       └── Sales/                 <-- Cashier sales domain
│   │           ├── Sale.cs            <-- Entities: Sale & SaleItem (ReceiptNo, Total, Change, Items)
│   │           └── ISalesService.cs   <-- Service interface for checkout & stock deduction
│   │
│   ├── 02. SmallMartApp.Infrastructure/ <-- [Class Library] The Outside World (Database & Devices)
│   │   ├── Persistence/               <-- Data storage implementations
│   │   │   └── SmallMartDbContext.cs  <-- EF Core SQLite DbContext (includes seed data)
│   │   ├── Hardware/                  <-- Real or simulated device drivers
│   │   │   ├── MockBarcodeScanner.cs  <-- Barcode scanner simulation (works without hardware)
│   │   │   └── FakeReceiptPrinter.cs  <-- Prints receipts to console/logs for testing
│   │   ├── Services/                  <-- Business logic implementations
│   │   │   ├── ProductService.cs      <-- Database queries & barcode validation
│   │   │   └── SalesService.cs        <-- Validates stock, calculates change & saves sales
│   │   └── DependencyInjection.cs     <-- Extension method AddInfrastructure() for DI container
│   │
│   └── 03. SmallMartApp.UI/            <-- [WPF Desktop Application] User Interface & Presentation
│       ├── ViewModels/                <-- MVVM state & command logic (CommunityToolkit.Mvvm)
│       │   ├── ViewModelBase.cs       <-- Base class for all ViewModels
│       │   ├── CartItemViewModel.cs   <-- Observable cart item model with auto-calculated subtotal
│       │   ├── PosCheckoutViewModel.cs<-- Cashier screen logic (barcode input, total, payment)
│       │   └── ProductListViewModel.cs<-- Inventory screen logic (product table, add item)
│       ├── Views/                     <-- Reusable UserControls & XAML screens
│       │   ├── PosCheckoutView.xaml   <-- Cashier checkout screen UI
│       │   ├── PosCheckoutView.xaml.cs<-- Cashier view code-behind
│       │   ├── ProductListView.xaml   <-- Inventory table and product entry form UI
│       │   └── ProductListView.xaml.cs<-- Inventory view code-behind
│       ├── MainWindow.xaml            <-- Main window shell with tab navigation
│       ├── MainWindow.xaml.cs         <-- Main window initialization & DataContext binding
│       ├── App.xaml                   <-- Application definition & global resources
│       └── App.xaml.cs                <-- Application startup, Host DI builder & DB initialization
│
└── tests/
    └── SmallMartApp.Tests/            <-- [xUnit Test Project]
        └── SalesServiceTests.cs       <-- In-memory SQLite tests for sales checkout & stock deduction
```

---

## 📖 Detailed Explanation: What Each Folder & File Is Used For

### 1. Root Files
* **`SmallMartApp.sln` & `SmallMartApp.slnx`**: The solution files that tie all 4 projects together. You open these in Visual Studio, Rider, or VS Code (C# Dev Kit).
* **`Directory.Build.props`**: A global MSBuild configuration file that applies settings across all projects in the solution (such as `.NET 8`, `<Nullable>enable</Nullable>`, and C# 12 features) without duplicating them in every `.csproj`.
* **`README.md`**: Project documentation, folder explanations, and getting-started guide.

---

### 2. `01. SmallMartApp.Core` (The Brain)
*Pure C# Class Library. Contains ZERO references to UI frameworks (no WPF/XAML) and ZERO references to databases (no EF Core). This keeps your business rules clean, fast, and 100% testable.*

* **`Common/`**: Contains shared foundational types.
  * `BaseEntity.cs`: The parent class for all models, providing an `Id` and `CreatedAt` timestamp.
  * `Result.cs`: Functional pattern for returning operation success/failure status and error messages without throwing expensive exceptions.
* **`Hardware/`**: Hardware contracts and interfaces.
  * `IBarcodeScanner.cs`: Defines the contract for barcode readers (`BarcodeScanned` event, `ConnectAsync`, `DisconnectAsync`).
  * `IReceiptPrinter.cs`: Defines the contract for printing customer receipts (`PrintReceiptAsync`).
* **`Features/`**: Grouped by business function (**Vertical Slice Architecture**):
  * **`Features/Products/`**:
    * `Product.cs`: Represents store inventory items (Barcode, Name, CostPrice, SellPrice, StockQuantity).
    * `IProductService.cs`: Contract for inventory actions (listing items, getting item by barcode, adding/updating items, deleting items).
  * **`Features/Sales/`**:
    * `Sale.cs` & `SaleItem.cs`: Entities representing completed transactions and line items in receipts.
    * `ISalesService.cs`: Contract for processing a checkout transaction and deducting inventory stock.

---

### 3. `02. SmallMartApp.Infrastructure` (The Outside World)
*Class Library that implements the contracts defined in `Core`. This is where all communication with external systems—such as databases, local files, and physical hardware devices—takes place.*

* **`Persistence/`**: Data access layer.
  * `SmallMartDbContext.cs`: Entity Framework Core database context configured for SQLite (`smallmart.db`). Sets up indexes (unique barcodes), table relationships, and default seed items for testing.
* **`Hardware/`**: Device drivers.
  * `MockBarcodeScanner.cs`: A simulated barcode scanner that triggers scan events via code or keyboard shortcuts. Allows full development without physical hardware.
  * `FakeReceiptPrinter.cs`: Formats receipts into readable text blocks printed to the debug console or logs.
* **`Services/`**: Business logic implementations.
  * `ProductService.cs`: Implements `IProductService` using EF Core database queries with duplicate barcode validation.
  * `SalesService.cs`: Implements `ISalesService`. Validates stock availability, calculates change, logs the sale to SQLite, deducts product quantities, and triggers the receipt printer.
* **`DependencyInjection.cs`**: Contains the extension method `AddInfrastructure()`, which cleanly registers the database context, hardware drivers, and services into the .NET DI container.

---

### 4. `03. SmallMartApp.UI` (The Screen)
*WPF Desktop Application. Contains all UI views, styling, and ViewModels following the **MVVM (Model-View-ViewModel)** design pattern.*

* **`ViewModels/`**: Holds UI state and user commands. Powered by `CommunityToolkit.Mvvm`.
  * `ViewModelBase.cs`: Common base class inheriting from `ObservableObject`.
  * `CartItemViewModel.cs`: Represents an item inside the shopping cart, providing live subtotal calculation when quantity changes.
  * `PosCheckoutViewModel.cs`: Drives the Cashier checkout screen. Handles barcode inputs, listens to the scanner, calculates total and change, and processes the sale.
  * `ProductListViewModel.cs`: Drives the Inventory screen. Manages the product list, form inputs for new products, and stock refresh.
* **`Views/`**: Visual XAML user controls and screens.
  * `PosCheckoutView.xaml` / `.cs`: Cashier checkout interface with a cart table, payment panel, and quick mock scan buttons.
  * `ProductListView.xaml` / `.cs`: Inventory management screen with a product data table and an "Add Product" form.
* **`MainWindow.xaml` / `.cs`**: The main desktop shell window featuring a top navigation bar and tabs to switch between the Cashier POS view and the Inventory view.
* **`App.xaml` / `.cs`**: The entry point of the desktop app. Sets up the Microsoft generic host, builds Dependency Injection, ensures the SQLite database is automatically generated on first run, and launches `MainWindow`.

---

### 5. `tests/SmallMartApp.Tests` (Quality Assurance)
*xUnit Test Project. Verifies that business logic and database interactions operate correctly.*

* **`SalesServiceTests.cs`**: Uses an in-memory SQLite database to test transactions: verifies that completing a sale properly deducts stock from inventory and calculates exact change.

---

## 🛠️ How to Build and Run

Open your terminal in `D:\RUPPClass\year4\OOAD\project\SmallMartApp`:

### Build the Solution:
```bash
dotnet build SmallMartApp.sln
```

### Launch the Desktop Application:
```bash
dotnet run --project "src/03. SmallMartApp.UI"
```

### Launch with Hot Reload (Auto-refreshes UI on file save):
```bash
dotnet watch --project "src/03. SmallMartApp.UI"
```

### Run Unit Tests:
```bash
dotnet test SmallMartApp.sln
```