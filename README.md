# SmallMartApp - Smart Mart POS & Inventory Management System

A modular, clean Windows Desktop architecture designed for small marts, supermarkets, and retail checkout kiosks. Built with **.NET 8 (WPF)** using **Feature-First / Vertical Slice Architecture**, **MVVM**, and **EF Core SQLite**.

---

## 🏛️ System Architecture Diagram (ERD)

```text
                              ┌─────────────┐
                              │  Category   │
                              └──────┬──────┘
                                     │ 1
                                     │ *
┌──────────────┐ 1          * ┌──────┴──────┐ *          1 ┌──────────────┐
│   Supplier   ├─────────────►│   Product   │◄─────────────┤   Customer   │
└──────────────┘              └──────┬──────┘              └──────┬───────┘
                                     │ 1                          │ 1
                                     │ *                          │ *
                              ┌──────┴──────┐ 1          * ┌──────┴───────┐
                              │  SaleItem   ├─────────────►│     Sale     │
                              └─────────────┘              └──────┬───────┘
                                                                  │ *
                                                                  │ 1
                                                           ┌──────┴───────┐
                                                           │ CashierShift │
                                                           └──────┬───────┘
                                                                  │ *
                                                                  │ 1
                                                           ┌──────┴───────┐
                                                           │     User     │
                                                           └──────────────┘
```

---

## 📁 Project Structure

```text
D:\RUPPClass\year4\OOAD\project\SmallMartApp/
│
├── SmallMartApp.sln                   <-- Solution file (Visual Studio, VS Code, CLI)
├── SmallMartApp.slnx                  <-- Modern XML solution file (.NET 8/10)
├── Directory.Build.props              <-- Solution-wide build configuration (net8.0, nullable, C# 12)
├── README.md                          <-- Project documentation & architecture guide
│
├── src/
│   │
│   ├── 01. SmallMartApp.Core/          <-- [Class Library] Pure C# (The Brain)
│   │   ├── Common/                    <-- BaseEntity (Id, CreatedAt), Result pattern
│   │   ├── Hardware/                  <-- IBarcodeScanner, IReceiptPrinter
│   │   └── Features/                  <-- Vertical Slices by Business Feature
│   │       ├── Products/              <-- Category.cs, Product.cs, IProductService.cs
│   │       ├── Sales/                 <-- Sale.cs, SaleItem.cs, PaymentMethod.cs, ISalesService.cs
│   │       ├── Customers/             <-- Customer.cs (Loyalty points & phone lookup)
│   │       ├── Suppliers/             <-- Supplier.cs, PurchaseOrder.cs, PurchaseOrderItem.cs
│   │       ├── Auth/                  <-- User.cs, UserRole.cs (Admin, Cashier)
│   │       └── Shifts/                <-- CashierShift.cs, ShiftStatus.cs (Open, Closed)
│   │
│   ├── 02. SmallMartApp.Infrastructure/ <-- [Class Library] Database & Devices
│   │   ├── Persistence/
│   │   │   └── SmallMartDbContext.cs  <-- EF Core SQLite (9 tables, relations, and seed data)
│   │   ├── Hardware/
│   │   │   ├── MockBarcodeScanner.cs  <-- Barcode scanner simulation (works without hardware)
│   │   │   └── FakeReceiptPrinter.cs  <-- Prints receipts to console/logs for testing
│   │   ├── Services/
│   │   │   ├── ProductService.cs      <-- Database queries & barcode validation
│   │   │   └── SalesService.cs        <-- Validates stock, calculates change & saves sales
│   │   └── DependencyInjection.cs     <-- Extension method AddInfrastructure() for DI container
│   │
│   └── 03. SmallMartApp.UI/            <-- [WPF Desktop Application] User Interface & Presentation
│       ├── ViewModels/
│       │   ├── ViewModelBase.cs
│       │   ├── CartItemViewModel.cs
│       │   ├── PosCheckoutViewModel.cs
│       │   └── ProductListViewModel.cs
│       ├── Views/
│       │   ├── PosCheckoutView.xaml / .cs
│       │   └── ProductListView.xaml / .cs
│       ├── MainWindow.xaml / .cs
│       ├── App.xaml / .cs
│       └── smallmart.db               <-- SQLite database (generated on first run)
│
└── tests/
    └── SmallMartApp.Tests/            <-- [xUnit Test Project]
        └── SalesServiceTests.cs       <-- In-memory SQLite checkout & stock deduction tests
```

---

## 🗄️ Database Tables Reference (9 Tables)

1. **`Categories`**: Product categories (Beverages, Snacks, Groceries).
2. **`Products`**: Inventory items with Barcode, Prices, StockQuantity, and LowStockAlertThreshold.
3. **`Customers`**: Customer accounts with Phone numbers and Loyalty points.
4. **`Suppliers`**: Vendors providing stock to the mart.
5. **`PurchaseOrders` & `PurchaseOrderItems`**: Restock orders tracking products received from suppliers.
6. **`Users`**: Mart staff accounts (Admins and Cashiers).
7. **`CashierShifts`**: Cash drawer shift tracking (Opening cash, Expected cash, Closing cash).
8. **`Sales`**: Completed checkout receipts (Total, Discount, Cash, Change, PaymentMethod).
9. **`SaleItems`**: Individual purchased items per sale receipt.

---

## 🛠️ How to Build and Run

```bash
# Build
dotnet build SmallMartApp.sln

# Run Desktop UI
dotnet run --project "src/03. SmallMartApp.UI"

# Run Tests
dotnet test SmallMartApp.sln
```