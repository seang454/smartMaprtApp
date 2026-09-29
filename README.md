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

## 🛠️ How to Create This Project From Scratch (Step-by-Step Tutorial)

If you are recreating or explaining this architecture for a course or team project, follow these complete setup steps:

### Step 1: Solution & Project Scaffolding
Create the directory and initialize the .NET solution with Clean Architecture layers:

```powershell
# 1. Create root directory
mkdir SmallMartApp
cd SmallMartApp

# 2. Create the main solution file
dotnet new sln -n SmallMartApp

# 3. Create Directory.Build.props to enforce C# 12 and nullable references across all projects
Set-Content Directory.Build.props @"
<Project>
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>12.0</LangVersion>
  </PropertyGroup>
</Project>
"@

# 4. Create the 3 architectural layers + 1 test project
dotnet new classlib -o "src/01. SmallMartApp.Core"
dotnet new classlib -o "src/02. SmallMartApp.Infrastructure"
dotnet new wpf -o "src/03. SmallMartApp.UI"
dotnet new xunit -o "tests/SmallMartApp.Tests"

# 5. Add all projects to the solution
dotnet sln add "src/01. SmallMartApp.Core"
dotnet sln add "src/02. SmallMartApp.Infrastructure"
dotnet sln add "src/03. SmallMartApp.UI"
dotnet sln add "tests/SmallMartApp.Tests"
```

---

### Step 2: Establish Inward Clean Architecture Dependencies

Enforce strict inward dependency flow (UI $\rightarrow$ Infrastructure $\rightarrow$ Core):

```powershell
# Infrastructure depends only on Core
dotnet add "src/02. SmallMartApp.Infrastructure" reference "src/01. SmallMartApp.Core"

# UI depends on Core and Infrastructure
dotnet add "src/03. SmallMartApp.UI" reference "src/01. SmallMartApp.Core"
dotnet add "src/03. SmallMartApp.UI" reference "src/02. SmallMartApp.Infrastructure"

# Tests depend on Core and Infrastructure
dotnet add "tests/SmallMartApp.Tests" reference "src/01. SmallMartApp.Core"
dotnet add "tests/SmallMartApp.Tests" reference "src/02. SmallMartApp.Infrastructure"
```

---

### Step 3: Install Required NuGet Packages

Install the production packages required for database persistence, MVVM, and QR generation:

```powershell
# 1. Infrastructure Packages (EF Core SQL Server, SQLite, QRCoder)
cd "src/02. SmallMartApp.Infrastructure"
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.11
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.11
dotnet add package QRCoder --version 1.8.0
cd ../..

# 2. UI Packages (CommunityToolkit.Mvvm & Microsoft.Extensions.Hosting)
cd "src/03. SmallMartApp.UI"
dotnet add package CommunityToolkit.Mvvm --version 8.3.2
dotnet add package Microsoft.Extensions.Hosting --version 8.0.1
cd ../..

# 3. Test Packages (xUnit & EF Core in-memory verification)
cd "tests/SmallMartApp.Tests"
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.11
dotnet add package Microsoft.NET.Test.Sdk --version 17.8.0
dotnet add package xunit --version 2.5.3
dotnet add package xunit.runner.visualstudio --version 2.5.3
cd ../..
```

---

### Step 4: Implement Core Domain Layer (`SmallMartApp.Core`)

1. **Common abstractions:**
   - Create `Common/BaseEntity.cs` containing `Id (int)` and `CreatedAt (DateTime)`.
   - Create `Common/Result.cs` providing generic `Result<T>` and `Result.Success() / Result.Failure()` patterns for deterministic error handling without runtime exceptions.
2. **Hardware abstractions:**
   - Define `Hardware/IBarcodeScanner.cs` (event `BarcodeScanned`).
   - Define `Hardware/IReceiptPrinter.cs` (`PrintReceiptAsync`).
3. **Vertical Slice Feature Domain Models:**
   - `Features/Products/`: `Category.cs`, `Product.cs`, `IProductService.cs`, `ICategoryService.cs`.
   - `Features/Suppliers/`: `Supplier.cs`, `PurchaseOrder.cs`, `PurchaseOrderItem.cs`, `ISupplierService.cs`.
   - `Features/Customers/`: `Customer.cs`, `ICustomerService.cs`.
   - `Features/Auth/`: `User.cs`, `UserRole.cs` (Admin, Cashier), `IAuthService.cs`, `IUserService.cs`.
   - `Features/Shifts/`: `CashierShift.cs`, `ShiftStatus.cs` (Open, Closed), `IShiftService.cs`.
   - `Features/Sales/`: `Sale.cs`, `SaleItem.cs`, `PaymentMethod.cs` (Cash, KHQR, Card), `ISalesService.cs`.
   - `Features/Payments/`: `IKhqrService.cs` (KHQR generation interface).
   - `Features/Dashboard/`: `IDashboardService.cs`.

---

### Step 5: Implement Infrastructure Layer (`SmallMartApp.Infrastructure`)

1. **Persistence with EF Core (`Persistence/SmallMartDbContext.cs`):**
   - Inherit from `DbContext`.
   - Declare `DbSet<T>` for all 10 entity models.
   - Configure relationships, unique indexes (`Barcode`, `PhoneNumber`, `Username`), decimal precisions `(18,2)`, and initial seed records in `OnModelCreating`.
2. **Hardware Peripherals:**
   - Implement `Hardware/MockBarcodeScanner.cs` to trigger barcode events programmatically for testing.
   - Implement `Hardware/FakeReceiptPrinter.cs` for file/console logging.
3. **Service Implementations:**
   - `Services/ProductService.cs`: Queries products and checks low stock thresholds.
   - `Services/SalesService.cs`: Coordinates atomic sales transactions, stock deduction validation, customer loyalty point calculation, and shift assignment.
   - `Services/ShiftService.cs`: Manages opening cash float, active shift status, and closing cash discrepancy math (`Discrepancy = ActualCash - ExpectedCash`).
   - `Services/KhqrService.cs`: Generates National Bank of Cambodia (NBC) Bakong TLV payloads with CRC-16 CCITT validation and QR bitmap rasterization.
4. **IoC Dependency Injection Extension (`DependencyInjection.cs`):**
   - Create `AddInfrastructure(this IServiceCollection services, string connectionString)` to register `SmallMartDbContext` with `ServiceLifetime.Transient` (preventing WPF multi-threading concurrency issues) and bind all service interfaces to their implementations.

---

### Step 6: Implement WPF Presentation Layer (`SmallMartApp.UI`)

1. **Configuration (`appsettings.json`):**
   - Set up `DefaultConnection` pointing to SQL Server Express or LocalDB.
   - In `SmallMartApp.UI.csproj`, ensure `<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>` is set on `appsettings.json`.
2. **Bootstrapper (`App.xaml.cs`):**
   - Build a generic host via `Host.CreateDefaultBuilder()`.
   - Register configuration, Infrastructure, ViewModels, and Windows into the `IServiceCollection`.
   - In `OnStartup()`, invoke `await db.Database.EnsureCreatedAsync()` to auto-migrate the database.
   - Launch `LoginWindow.Show()`.
3. **ViewModels (`CommunityToolkit.Mvvm`):**
   - Inherit from `ObservableObject`.
   - Use `[ObservableProperty]` on backing fields (generates reactive `INotifyPropertyChanged` properties).
   - Use `[RelayCommand]` on methods (generates async `ICommand` handlers with `CanExecute` validation).
   - Implement `MainViewModel` with `CurrentView` property to handle dynamic screen switching.
   - Implement `PosCheckoutViewModel`, `ProductListViewModel`, `ShiftViewModel`, `CategoryViewModel`, `CustomerViewModel`, `SupplierViewModel`, `UserViewModel`, `LoginViewModel`.
4. **Views & DataTemplates:**
   - Create modular XAML `UserControl`s for each feature view.
   - In `App.xaml`, declare `DataTemplate` mappings connecting each ViewModel type directly to its corresponding View UserControl.
   - Implement `WpfReceiptPrinter` to open an interactive thermal receipt preview window upon sale completion.

---

### Step 7: Automated Testing & Verification (`SmallMartApp.Tests`)

1. Create `SalesServiceTests.cs` using **xUnit** and **In-Memory DbContext**:
   - Verify cart subtotal, discount calculation, and change mathematics.
   - Verify that confirming a checkout decrements stock quantities atomically.
   - Verify that invalid barcodes or insufficient cash throw deterministic `Result.Failure` messages instead of crashing.
2. Run tests to confirm integrity:
   ```powershell
   dotnet test SmallMartApp.sln
   ```

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