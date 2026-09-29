# SmallMartApp - Smart Mart POS & Inventory Management System

A production-grade, modular Windows Desktop Point of Sale (POS) and inventory management architecture designed for small marts, supermarkets, and retail checkout kiosks. Built with **.NET 8 (WPF)** using **Clean Architecture / Vertical Slice Architecture**, **MVVM (CommunityToolkit.Mvvm)**, and **Entity Framework Core 8**.

---

## 🏛️ System Architecture Diagram (ERD)

```text
                               ┌─────────────┐
                               │  Category   │
                               └──────┬──────┘
                                      │ 1
                                      │ * (CategoryId)
┌──────────────┐ 1          * ┌──────┴──────┐ * (CustomerId) 1 ┌──────────────┐
│   Supplier   ├─────────────►│   Product   │◄─────────────────┤   Customer   │
└──────┬───────┘ (SupplierId) └──────┬──────┘                  └──────┬───────┘
       │ 1                           │ 1                              │ 1
       │ *                           │ *                              │ *
┌──────┴───────┐              ┌──────┴──────┐ 1             *  ┌──────┴───────┐
│PurchaseOrder │              │  SaleItem   ├─────────────────►│     Sale     │
└──────┬───────┘              └─────────────┘   (SaleId)       └──────┬───────┘
       │ 1                                                            │ *
       │ *                                                            │ 1 (ShiftId)
┌──────┴──────────┐                                            ┌──────┴───────┐
│PurchaseOrderItem│                                            │ CashierShift │
└─────────────────┘                                            └──────┬───────┘
                                                                      │ *
                                                                      │ 1 (UserId)
                                                               ┌──────┴───────┐
                                                               │     User     │
                                                               └──────────────┘
```

---

## 📁 Project Structure

```text
SmallMartApp/
│
├── SmallMartApp.sln                   <-- Solution file (Visual Studio, VS Code, CLI)
├── SmallMartApp.slnx                  <-- Modern XML solution file (.NET 8/10)
├── Directory.Build.props              <-- Solution-wide build configuration (net8.0, nullable, C# 12)
├── README.md                          <-- Project documentation & setup guide
│
├── docs/                              <-- Academic documentation (.docx and .pdf)
├── scripts/
│   ├── SeedData_10RecordsPerTable.sql <-- Full SQL Server seed script (10 records/table)
│   └── build_complete_docx.py         <-- Documentation generator script
│
├── src/
│   │
│   ├── 01. SmallMartApp.Core/          <-- [Class Library] Pure C# (The Domain Brain)
│   │   ├── Common/                    <-- BaseEntity (Id, CreatedAt), Result<T> pattern
│   │   ├── Hardware/                  <-- IBarcodeScanner, IReceiptPrinter interfaces
│   │   └── Features/                  <-- Vertical Slices by Business Feature
│   │       ├── Products/              <-- Category.cs, Product.cs, IProductService.cs
│   │       ├── Sales/                 <-- Sale.cs, SaleItem.cs, PaymentMethod.cs, ISalesService.cs
│   │       ├── Customers/             <-- Customer.cs, ICustomerService.cs (Loyalty points)
│   │       ├── Suppliers/             <-- Supplier.cs, PurchaseOrder.cs, PurchaseOrderItem.cs
│   │       ├── Auth/                  <-- User.cs, UserRole.cs (Admin, Cashier), IAuthService.cs
│   │       ├── Shifts/                <-- CashierShift.cs, ShiftStatus.cs, IShiftService.cs
│   │       ├── Payments/              <-- IKhqrService.cs (Bakong KHQR payment engine)
│   │       └── Dashboard/             <-- IDashboardService.cs (Sales & stock KPIs)
│   │
│   ├── 02. SmallMartApp.Infrastructure/ <-- [Class Library] Data Persistence & Peripherals
│   │   ├── Persistence/
│   │   │   └── SmallMartDbContext.cs  <-- EF Core DbContext (10 tables, relations & seeding)
│   │   ├── Hardware/
│   │   │   ├── MockBarcodeScanner.cs  <-- Virtual barcode scanner driver
│   │   │   └── FakeReceiptPrinter.cs  <-- Console/file receipt logging driver
│   │   ├── Services/
│   │   │   ├── ProductService.cs      <-- Product queries, barcodes & stock management
│   │   │   ├── SalesService.cs        <-- Atomic checkout transaction, stock decrements
│   │   │   ├── ShiftService.cs        <-- Float opening, expected cash & shift reconciliation
│   │   │   └── KhqrService.cs         <-- EMVCo KHQR byte generator (CRC-16 CCITT)
│   │   └── DependencyInjection.cs     <-- IoC container registrations (AddInfrastructure)
│   │
│   └── 03. SmallMartApp.UI/            <-- [WPF Desktop Application] User Interface
│       ├── appsettings.json           <-- Connection strings & configuration
│       ├── ViewModels/                <-- CommunityToolkit.Mvvm ViewModels
│       │   ├── MainViewModel.cs       <-- View navigation coordinator
│       │   ├── LoginViewModel.cs      <-- User authentication & session state
│       │   ├── PosCheckoutViewModel.cs<-- Frontline checkout, cart & payment logic
│       │   ├── ProductListViewModel.cs<-- Inventory management & stock threshold alerts
│       │   ├── ShiftViewModel.cs      <-- Cash drawer opening float & shift closure
│       │   └── DashboardViewModel.cs  <-- Sales KPIs and revenue metrics
│       ├── Views/                     <-- Declarative XAML user controls & dialogs
│       ├── Services/                  <-- WpfReceiptPrinter (Visual thermal slip window)
│       └── App.xaml / .cs             <-- DI container bootstrapper & DB auto-creation
│
└── tests/
    └── SmallMartApp.Tests/            <-- [xUnit Test Project]
        └── SalesServiceTests.cs       <-- Automated checkout, math & stock deduction tests
```

---

## 🗄️ Database Tables Reference (10 Relational Tables)

| # | Table Name | Primary Key | Foreign Keys | Key Attributes & Constraints |
|---|:---|:---|:---|:---|
| 1 | **`Categories`** | `Id (int)` | *None* | `Name` (Required), `Description`, `CreatedAt` |
| 2 | **`Suppliers`** | `Id (int)` | *None* | `CompanyName`, `PhoneNumber`, `ContactPerson`, `Address` |
| 3 | **`Products`** | `Id (int)` | `CategoryId`, `SupplierId` | `Barcode` (Unique), `Name`, `CostPrice`, `SellPrice`, `StockQuantity`, `LowStockAlertThreshold` |
| 4 | **`Customers`** | `Id (int)` | *None* | `PhoneNumber` (Unique), `FullName`, `Points` (Loyalty) |
| 5 | **`Users`** | `Id (int)` | *None* | `Username` (Unique), `FullName`, `PasswordHash`, `Role` (Admin/Cashier), `WorkingShift` |
| 6 | **`CashierShifts`** | `Id (int)` | `UserId` | `StartingCash`, `ExpectedCash`, `ActualCash`, `Status` (Open/Closed), `StartTime`, `EndTime` |
| 7 | **`Sales`** | `Id (int)` | `ShiftId`, `CustomerId` | `ReceiptNumber` (Unique), `TotalAmount`, `DiscountAmount`, `CashReceived`, `ChangeGiven`, `PaymentMethod` |
| 8 | **`SaleItems`** | `Id (int)` | `SaleId` (Cascade), `ProductId` | `ProductName`, `UnitPrice`, `Quantity`, `Subtotal` |
| 9 | **`PurchaseOrders`** | `Id (int)` | `SupplierId` | `TotalCost`, `OrderDate` |
| 10 | **`PurchaseOrderItems`** | `Id (int)` | `PurchaseOrderId` (Cascade), `ProductId` | `ProductName`, `UnitCost`, `Quantity`, `Subtotal` |

---

## 💻 System Requirements & Prerequisites

Ensure the following tools are installed on your machine before running the project:

- **Operating System:** Microsoft Windows 10 or Windows 11 (64-bit) *(Required for WPF desktop rendering)*
- **.NET SDK:** [.NET 8.0 SDK (v8.0.x)](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Database Engine:** **Microsoft SQL Server** (SQL Server Express, Developer Edition, or LocalDB)
- **IDE (Recommended):** [Visual Studio 2022](https://visualstudio.microsoft.com/) (v17.8 or later) with the **".NET desktop development"** workload selected, or **JetBrains Rider** / **VS Code** with the C# Dev Kit extension.
- **Database Management Tool (Optional):** [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) or Azure Data Studio.

---

## ⚙️ Database Configuration

The application connects to SQL Server using the connection string defined in:
`src/03. SmallMartApp.UI/appsettings.json`

Verify or update the connection string to match your local SQL Server instance:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=SmallMartDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;"
  }
}
```

> **Using LocalDB instead of SQL Server Express?**  
> If you have Visual Studio's built-in LocalDB, update your `DefaultConnection` string to:  
> `"Server=(localdb)\\mssqllocaldb;Database=SmallMartDb;Trusted_Connection=True;MultipleActiveResultSets=true;"`

---

## 🚀 How to Build and Run the Project

### Method A: Using Command Line (CLI / PowerShell)

1. **Open PowerShell or Command Prompt** and navigate to the project directory:
   ```powershell
   cd d:\RUPPClass\year4\OOAD\project\SmallMartApp
   ```

2. **Restore NuGet dependencies and build the entire solution:**
   ```powershell
   dotnet build SmallMartApp.sln
   ```

3. **Launch the WPF Desktop Application:**
   ```powershell
   dotnet run --project "src/03. SmallMartApp.UI"
   ```

4. **Run Automated Unit Tests:**
   ```powershell
   dotnet test SmallMartApp.sln
   ```

---

### Method B: Using Visual Studio 2022

1. Double-click **`SmallMartApp.sln`** to open the project in Visual Studio.
2. In the **Solution Explorer** panel, expand the `src` folder.
3. Right-click on **`03. SmallMartApp.UI`** and select **"Set as Startup Project"**.
4. Set the configuration to **Debug** and platform to **Any CPU**.
5. Press **`F5`** (or click the green **Start** button) to build and run the application.

---

## 🌱 Database Initialization & Sample Data Seeding

### 1. Automatic Database Creation
On the first application run, the system automatically executes:
```csharp
await db.Database.EnsureCreatedAsync();
```
This automatically creates the `SmallMartDb` database, configures the schema for all 10 tables, and inserts initial seed data (default users, sample categories, and products).

### 2. Optional: Seed 10 Realistic Records per Table
For comprehensive testing, grading, and demonstration, a complete SQL seeding script is provided in the repository:

1. Open **SQL Server Management Studio (SSMS)**.
2. Connect to your SQL Server instance (e.g. `localhost\SQLEXPRESS`).
3. Open the file:
   [`scripts/SeedData_10RecordsPerTable.sql`](file:///d:/RUPPClass/year4/OOAD/project/SmallMartApp/scripts/SeedData_10RecordsPerTable.sql)
4. Execute the script (`F5`).
5. This populates **10 correlated records for every single table** (categories, products, suppliers, customers, shifts, purchase orders, and sales).

---

## 🔑 Default Login Accounts

When the application starts, the **Login Window** appears. Use either of the pre-configured credentials below:

| Role | Username | Password | Features & Access Level |
|:---|:---|:---|:---|
| **Administrator** | `admin` | `admin123` | Full administrative access: Dashboard KPIs, Inventory, Shift Audits, User Management, Suppliers, and Customers |
| **Cashier** | `cashier1` | `123456` | Frontline checkout: POS Cart, Barcode Scanning, KHQR payments, Customer points, Cash Drawer shift float |

---

## 🛒 Step-by-Step Feature Walkthrough

1. **Open a Cashier Shift:**
   - Log in using `cashier1` / `123456`.
   - On the **Shift** screen, enter your starting cash float (e.g. `$50.00`) and click **Open Shift**.
2. **Perform POS Checkout:**
   - Go to the **POS Checkout** screen.
   - Scan or enter sample barcodes in the input field:
     - `885012401` — Coca Cola Can 330ml ($0.65)
     - `885012402` — Mineral Water Vital 500ml ($0.35)
     - `885012403` — Lay's Potato Chips ($1.25)
     - `885012404` — Mama Instant Noodles ($0.50)
   - Alternatively, click any product card on screen to add it to the cart.
3. **Customer Loyalty Points:**
   - Enter a registered customer phone number (e.g., `012888999`) to link the customer and accrue points (1 point per dollar spent).
4. **Choose Payment Method:**
   - **Cash:** Enter the cash received. The system will automatically calculate the change to give back.
   - **KHQR:** Generates a real-time National Bank of Cambodia (NBC) Bakong dynamic QR code with exact amount and CRC-16 checksum.
5. **Complete Checkout & Virtual Thermal Receipt:**
   - Click **Complete Checkout**.
   - An interactive **Virtual Thermal Receipt Window** pops up, simulating an ESC/POS 80mm thermal receipt printer with store headers, itemized breakdown, and barcode.
6. **Shift Closure & Cash Audit:**
   - Navigate to the **Shift** view.
   - Input the physically counted cash from the drawer.
   - The system instantly calculates discrepancy (`Actual - Expected`) and seals the shift audit record.

---

## ❓ Troubleshooting & FAQs

### Q1: "Cannot open database 'SmallMartDb' requested by the login" or SQL Connection Timeout
- **Cause:** SQL Server service is stopped or the instance name does not match.
- **Fix:**
  1. Press `Win + R`, type `services.msc`, and hit Enter.
  2. Find **SQL Server (SQLEXPRESS)** and make sure its status is **Running**.
  3. If your SQL Server instance is named differently (e.g., `MSSQLSERVER` or `.`), update `Server=localhost` or `Server=.` in `src/03. SmallMartApp.UI/appsettings.json`.

### Q2: "WPF / Desktop applications can only run on Windows"
- **Cause:** WPF is a Windows-only desktop framework.
- **Fix:** SmallMartApp requires Windows 10 or Windows 11. If on macOS or Linux, run it inside a Windows virtual machine (e.g., Parallels or VMware).

### Q3: "The SDK 'Microsoft.NET.Sdk' specified could not be found"
- **Cause:** .NET 8.0 SDK is missing or outdated.
- **Fix:** Download and install the latest [.NET 8.0 SDK x64](https://dotnet.microsoft.com/download/dotnet/8.0), then restart your terminal or IDE.