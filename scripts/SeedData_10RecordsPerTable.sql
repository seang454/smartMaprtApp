-- ============================================================================
-- SMART MART MANAGEMENT SYSTEM - SAMPLE DATA SEED SCRIPT
-- Target Database: SmallMartDb (SQL Server / SQLEXPRESS)
-- Description: Inserts exactly 10 realistic supermarket records for EACH of
--              the 10 tables with matched Foreign Key relationships.
-- ============================================================================

USE [SmallMartDb];
GO

-- 1. Temporarily disable foreign key constraints to allow clean reseeding
EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all";
GO

-- 2. Clear existing sample data in reverse dependency order
DELETE FROM [SaleItems];
DELETE FROM [PurchaseOrderItems];
DELETE FROM [Sales];
DELETE FROM [PurchaseOrders];
DELETE FROM [CashierShifts];
DELETE FROM [Products];
DELETE FROM [Customers];
DELETE FROM [Suppliers];
DELETE FROM [Categories];
DELETE FROM [Users];
GO

-- ============================================================================
-- 1. CATEGORIES (10 Records)
-- ============================================================================
SET IDENTITY_INSERT [Categories] ON;
INSERT INTO [Categories] ([Id], [Name], [Description], [CreatedAt])
VALUES
(1, N'Beverages', N'Soft drinks, mineral water, tea, coffee, and fruit juices', GETUTCDATE()),
(2, N'Snacks & Confectionery', N'Potato chips, biscuits, chocolates, candies, and dried fruits', GETUTCDATE()),
(3, N'Bakery & Bread', N'Freshly baked artisan bread, sandwich bread, buns, and pastries', GETUTCDATE()),
(4, N'Dairy & Eggs', N'Pasteurized milk, farm fresh eggs, cheese, butter, and yogurt', GETUTCDATE()),
(5, N'Groceries & Rice', N'Jasmine rice, cooking oil, sauces, pasta, seasonings, and flour', GETUTCDATE()),
(6, N'Fresh Produce', N'Local and imported fresh fruits, green vegetables, and herbs', GETUTCDATE()),
(7, N'Meat & Seafood', N'Fresh chicken cuts, prime beef, pork belly, prawns, and fish fillets', GETUTCDATE()),
(8, N'Canned & Preserved Foods', N'Canned tuna, sweet corn, baked beans, canned peaches, and soups', GETUTCDATE()),
(9, N'Personal Care & Beauty', N'Shampoo, body wash, bath soap, toothpaste, and face cleansers', GETUTCDATE()),
(10, N'Household & Cleaning', N'Laundry detergents, dishwashing liquids, surface cleaners, and paper', GETUTCDATE());
SET IDENTITY_INSERT [Categories] OFF;
GO

-- ============================================================================
-- 2. SUPPLIERS (10 Records)
-- ============================================================================
SET IDENTITY_INSERT [Suppliers] ON;
INSERT INTO [Suppliers] ([Id], [CompanyName], [ContactPerson], [PhoneNumber], [Address], [CreatedAt])
VALUES
(1, N'Phnom Penh Distribution Co.', N'Sokha Chan', N'012345678', N'St 271, Toul Tum Poung, Phnom Penh', GETUTCDATE()),
(2, N'Angkor Beverage Logistics', N'Dara Vuth', N'015234567', N'Russian Blvd, Toul Kork, Phnom Penh', GETUTCDATE()),
(3, N'Mekong Dairy Farm Supply', N'Sreymao Tep', N'016345678', N'Hanoi Road, Sen Sok, Phnom Penh', GETUTCDATE()),
(4, N'Golden Harvest Agriculture', N'Bopha Khem', N'017456789', N'National Road 1, Kandal Province', GETUTCDATE()),
(5, N'Royal Snack & Candy Distributors', N'Vicheka Heng', N'018567890', N'Norodom Blvd, Daun Penh, Phnom Penh', GETUTCDATE()),
(6, N'Star Groceries Trading Ltd', N'Sovannarith Mao', N'019678901', N'National Road 2, Chbar Ampov, Phnom Penh', GETUTCDATE()),
(7, N'Premier Fresh Meat & Poultry', N'Chantou Lay', N'070789012', N'St 598, Russey Keo, Phnom Penh', GETUTCDATE()),
(8, N'Asia Pacific Consumer Goods', N'Mengly Ung', N'077890123', N'Mao Tse Toung Blvd, Chamkarmon, Phnom Penh', GETUTCDATE()),
(9, N'CleanHome Chemical Solutions', N'Narin Rath', N'088901234', N'St 310, Boeung Keng Kang 1, Phnom Penh', GETUTCDATE()),
(10, N'Global Pacific Import-Export', N'Vibol Seng', N'093012345', N'Veng Sreng Blvd, Por Senchey, Phnom Penh', GETUTCDATE());
SET IDENTITY_INSERT [Suppliers] OFF;
GO

-- ============================================================================
-- 3. CUSTOMERS (10 Records)
-- ============================================================================
SET IDENTITY_INSERT [Customers] ON;
INSERT INTO [Customers] ([Id], [FullName], [PhoneNumber], [Points], [CreatedAt])
VALUES
(1, N'Chea Mengly', N'012888999', 150, GETUTCDATE()),
(2, N'Keo Pich', N'015777888', 240, GETUTCDATE()),
(3, N'Heng Sopheak', N'016666777', 80, GETUTCDATE()),
(4, N'Chan Vanda', N'017555666', 320, GETUTCDATE()),
(5, N'Tep Bopha', N'018444555', 50, GETUTCDATE()),
(6, N'Kim Sokunthea', N'019333444', 190, GETUTCDATE()),
(7, N'Mao Piseth', N'070222333', 410, GETUTCDATE()),
(8, N'Seng Ratana', N'077111222', 95, GETUTCDATE()),
(9, N'Ou Vicheka', N'088000111', 125, GETUTCDATE()),
(10, N'Ly David', N'093999000', 500, GETUTCDATE());
SET IDENTITY_INSERT [Customers] OFF;
GO

-- ============================================================================
-- 4. USERS (10 Records)
-- Role: 'Admin' (Manager) or 'Cashier'
-- WorkingShift: 'Morning', 'Afternoon', 'Night', 'FullTime'
-- ============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[Users]') AND name = 'WorkingShift'
)
BEGIN
    ALTER TABLE [Users] ADD [WorkingShift] NVARCHAR(50) NOT NULL CONSTRAINT DF_Users_WorkingShift DEFAULT 'Morning';
END
GO

SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([Id], [Username], [FullName], [WorkingShift], [PasswordHash], [Role], [IsActive], [CreatedAt])
VALUES
(1, N'admin', N'Chea Sovann', N'FullTime', N'admin123', N'Admin', 1, GETUTCDATE()),
(2, N'manager1', N'Sokha Chan', N'Morning', N'manager123', N'Admin', 1, GETUTCDATE()),
(3, N'supervisor1', N'Dara Vuth', N'Afternoon', N'super123', N'Admin', 1, GETUTCDATE()),
(4, N'cashier1', N'Sreymao Tep', N'Morning', N'123456', N'Cashier', 1, GETUTCDATE()),
(5, N'cashier2', N'Bopha Khem', N'Afternoon', N'123456', N'Cashier', 1, GETUTCDATE()),
(6, N'cashier3', N'Vicheka Heng', N'Night', N'123456', N'Cashier', 1, GETUTCDATE()),
(7, N'cashier4', N'Sovannarith Mao', N'Morning', N'123456', N'Cashier', 1, GETUTCDATE()),
(8, N'cashier5', N'Chantou Lay', N'Afternoon', N'123456', N'Cashier', 1, GETUTCDATE()),
(9, N'cashier6', N'Mengly Ung', N'Night', N'123456', N'Cashier', 1, GETUTCDATE()),
(10, N'trainee1', N'Vibol Seng', N'Morning', N'123456', N'Cashier', 1, GETUTCDATE());
SET IDENTITY_INSERT [Users] OFF;
GO

-- ============================================================================
-- 5. PRODUCTS (10 Records)
-- ============================================================================
SET IDENTITY_INSERT [Products] ON;
INSERT INTO [Products] ([Id], [Barcode], [Name], [CategoryId], [SupplierId], [CostPrice], [SellPrice], [StockQuantity], [LowStockAlertThreshold], [IsActive], [CreatedAt])
VALUES
(1, N'885012401', N'Coca Cola Can 330ml', 1, 2, 0.40, 0.65, 120, 10, 1, GETUTCDATE()),
(2, N'885012402', N'Mineral Water Vital 500ml', 1, 2, 0.15, 0.35, 200, 15, 1, GETUTCDATE()),
(3, N'885012403', N'Lay''s Potato Chips Classic 75g', 2, 5, 0.80, 1.25, 45, 8, 1, GETUTCDATE()),
(4, N'885012404', N'Mama Instant Noodles Chicken', 5, 6, 0.25, 0.50, 150, 20, 1, GETUTCDATE()),
(5, N'885012405', N'Whole Wheat Toast Bread 400g', 3, 4, 1.10, 1.75, 30, 5, 1, GETUTCDATE()),
(6, N'885012406', N'Dutch Mill Fresh Milk 1L', 4, 3, 1.50, 2.30, 40, 8, 1, GETUTCDATE()),
(7, N'885012407', N'Malys Angkor Jasmine Rice 5kg', 5, 6, 4.20, 5.80, 50, 5, 1, GETUTCDATE()),
(8, N'885012408', N'Fresh Farm Organic Eggs 10-Pack', 4, 3, 1.20, 1.80, 60, 10, 1, GETUTCDATE()),
(9, N'885012409', N'Sunlight Dishwashing Lime 750ml', 10, 9, 1.10, 1.65, 35, 5, 1, GETUTCDATE()),
(10, N'885012410', N'Ayam Brand Canned Tuna 185g', 8, 1, 1.30, 1.95, 70, 10, 1, GETUTCDATE());
SET IDENTITY_INSERT [Products] OFF;
GO

-- ============================================================================
-- 6. CASHIER SHIFTS (10 Records)
-- Status: 'Open' or 'Closed'
-- ============================================================================
SET IDENTITY_INSERT [CashierShifts] ON;
INSERT INTO [CashierShifts] ([Id], [UserId], [StartTime], [EndTime], [StartingCash], [ExpectedCash], [ActualCash], [Status], [CreatedAt])
VALUES
(1, 4, '2026-09-20 07:00:00', '2026-09-20 15:00:00', 100.00, 350.00, 350.00, N'Closed', GETUTCDATE()),
(2, 5, '2026-09-20 15:00:00', '2026-09-20 22:00:00', 100.00, 420.50, 420.50, N'Closed', GETUTCDATE()),
(3, 4, '2026-09-21 07:00:00', '2026-09-21 15:00:00', 100.00, 280.00, 280.00, N'Closed', GETUTCDATE()),
(4, 6, '2026-09-21 15:00:00', '2026-09-21 22:00:00', 100.00, 510.75, 510.75, N'Closed', GETUTCDATE()),
(5, 4, '2026-09-22 07:00:00', '2026-09-22 15:00:00', 100.00, 395.20, 395.00, N'Closed', GETUTCDATE()),
(6, 5, '2026-09-22 15:00:00', '2026-09-22 22:00:00', 100.00, 460.00, 460.00, N'Closed', GETUTCDATE()),
(7, 7, '2026-09-23 07:00:00', '2026-09-23 15:00:00', 100.00, 315.50, 315.50, N'Closed', GETUTCDATE()),
(8, 8, '2026-09-23 15:00:00', '2026-09-23 22:00:00', 100.00, 480.00, 480.00, N'Closed', GETUTCDATE()),
(9, 4, '2026-09-24 07:00:00', '2026-09-24 15:00:00', 100.00, 410.00, 410.00, N'Closed', GETUTCDATE()),
(10, 4, '2026-09-25 07:00:00', NULL, 100.00, 100.00, NULL, N'Open', GETUTCDATE());
SET IDENTITY_INSERT [CashierShifts] OFF;
GO

-- ============================================================================
-- 7. SALES (10 Records)
-- PaymentMethod: 'Cash', 'KHQR', 'CreditCard'
-- ============================================================================
SET IDENTITY_INSERT [Sales] ON;
INSERT INTO [Sales] ([Id], [ReceiptNumber], [ShiftId], [CustomerId], [TotalAmount], [DiscountAmount], [CashReceived], [ChangeGiven], [PaymentMethod], [CreatedAt])
VALUES
(1, N'REC-20260920-001', 1, 1, 1.30, 0.00, 5.00, 3.70, N'Cash', GETUTCDATE()),
(2, N'REC-20260920-002', 1, 2, 11.60, 0.00, 11.60, 0.00, N'KHQR', GETUTCDATE()),
(3, N'REC-20260920-003', 2, 3, 3.75, 0.00, 10.00, 6.25, N'Cash', GETUTCDATE()),
(4, N'REC-20260921-004', 3, 4, 9.20, 0.50, 10.00, 0.80, N'Cash', GETUTCDATE()),
(5, N'REC-20260921-005', 4, 5, 3.50, 0.00, 3.50, 0.00, N'KHQR', GETUTCDATE()),
(6, N'REC-20260922-006', 5, 6, 5.40, 0.00, 10.00, 4.60, N'Cash', GETUTCDATE()),
(7, N'REC-20260922-007', 6, 7, 3.30, 0.00, 3.30, 0.00, N'CreditCard', GETUTCDATE()),
(8, N'REC-20260923-008', 7, 8, 9.75, 0.00, 20.00, 10.25, N'Cash', GETUTCDATE()),
(9, N'REC-20260923-009', 8, 9, 3.00, 0.00, 3.00, 0.00, N'KHQR', GETUTCDATE()),
(10, N'REC-20260924-010', 9, 10, 3.50, 0.00, 5.00, 1.50, N'Cash', GETUTCDATE());
SET IDENTITY_INSERT [Sales] OFF;
GO

-- ============================================================================
-- 8. SALE ITEMS (10 Records)
-- ============================================================================
SET IDENTITY_INSERT [SaleItems] ON;
INSERT INTO [SaleItems] ([Id], [SaleId], [ProductId], [ProductName], [Quantity], [UnitPrice], [CreatedAt])
VALUES
(1, 1, 1, N'Coca Cola Can 330ml', 2, 0.65, GETUTCDATE()),
(2, 2, 7, N'Malys Angkor Jasmine Rice 5kg', 2, 5.80, GETUTCDATE()),
(3, 3, 3, N'Lay''s Potato Chips Classic 75g', 3, 1.25, GETUTCDATE()),
(4, 4, 6, N'Dutch Mill Fresh Milk 1L', 4, 2.30, GETUTCDATE()),
(5, 5, 5, N'Whole Wheat Toast Bread 400g', 2, 1.75, GETUTCDATE()),
(6, 6, 8, N'Fresh Farm Organic Eggs 10-Pack', 3, 1.80, GETUTCDATE()),
(7, 7, 9, N'Sunlight Dishwashing Lime 750ml', 2, 1.65, GETUTCDATE()),
(8, 8, 10, N'Ayam Brand Canned Tuna 185g', 5, 1.95, GETUTCDATE()),
(9, 9, 4, N'Mama Instant Noodles Chicken', 6, 0.50, GETUTCDATE()),
(10, 10, 2, N'Mineral Water Vital 500ml', 10, 0.35, GETUTCDATE());
SET IDENTITY_INSERT [SaleItems] OFF;
GO

-- ============================================================================
-- 9. PURCHASE ORDERS (10 Records)
-- ============================================================================
SET IDENTITY_INSERT [PurchaseOrders] ON;
INSERT INTO [PurchaseOrders] ([Id], [SupplierId], [TotalCost], [OrderDate], [CreatedAt])
VALUES
(1, 1, 260.00, '2026-09-01 09:00:00', GETUTCDATE()),
(2, 2, 100.00, '2026-09-03 10:15:00', GETUTCDATE()),
(3, 3, 225.00, '2026-09-05 11:30:00', GETUTCDATE()),
(4, 4, 132.00, '2026-09-08 14:00:00', GETUTCDATE()),
(5, 5, 160.00, '2026-09-10 08:45:00', GETUTCDATE()),
(6, 6, 336.00, '2026-09-12 13:20:00', GETUTCDATE()),
(7, 7, 100.00, '2026-09-15 15:10:00', GETUTCDATE()),
(8, 8, 90.00, '2026-09-18 16:00:00', GETUTCDATE()),
(9, 9, 143.00, '2026-09-20 10:30:00', GETUTCDATE()),
(10, 10, 300.00, '2026-09-22 11:00:00', GETUTCDATE());
SET IDENTITY_INSERT [PurchaseOrders] OFF;
GO

-- ============================================================================
-- 10. PURCHASE ORDER ITEMS (10 Records)
-- ============================================================================
SET IDENTITY_INSERT [PurchaseOrderItems] ON;
INSERT INTO [PurchaseOrderItems] ([Id], [PurchaseOrderId], [ProductId], [ProductName], [Quantity], [UnitCost], [CreatedAt])
VALUES
(1, 1, 10, N'Ayam Brand Canned Tuna 185g', 200, 1.30, GETUTCDATE()),
(2, 2, 1, N'Coca Cola Can 330ml', 250, 0.40, GETUTCDATE()),
(3, 3, 6, N'Dutch Mill Fresh Milk 1L', 150, 1.50, GETUTCDATE()),
(4, 4, 5, N'Whole Wheat Toast Bread 400g', 120, 1.10, GETUTCDATE()),
(5, 5, 3, N'Lay''s Potato Chips Classic 75g', 200, 0.80, GETUTCDATE()),
(6, 6, 7, N'Malys Angkor Jasmine Rice 5kg', 80, 4.20, GETUTCDATE()),
(7, 7, 4, N'Mama Instant Noodles Chicken', 400, 0.25, GETUTCDATE()),
(8, 8, 2, N'Mineral Water Vital 500ml', 600, 0.15, GETUTCDATE()),
(9, 9, 9, N'Sunlight Dishwashing Lime 750ml', 130, 1.10, GETUTCDATE()),
(10, 10, 8, N'Fresh Farm Organic Eggs 10-Pack', 250, 1.20, GETUTCDATE());
SET IDENTITY_INSERT [PurchaseOrderItems] OFF;
GO

-- 3. Re-enable all foreign key constraints and verify integrity
EXEC sp_MSforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all";
GO

-- 4. Verification Check: Print counts of each table
SELECT 'Categories' AS [Table], COUNT(*) AS [RecordCount] FROM [Categories]
UNION ALL SELECT 'Suppliers', COUNT(*) FROM [Suppliers]
UNION ALL SELECT 'Customers', COUNT(*) FROM [Customers]
UNION ALL SELECT 'Users', COUNT(*) FROM [Users]
UNION ALL SELECT 'Products', COUNT(*) FROM [Products]
UNION ALL SELECT 'CashierShifts', COUNT(*) FROM [CashierShifts]
UNION ALL SELECT 'Sales', COUNT(*) FROM [Sales]
UNION ALL SELECT 'SaleItems', COUNT(*) FROM [SaleItems]
UNION ALL SELECT 'PurchaseOrders', COUNT(*) FROM [PurchaseOrders]
UNION ALL SELECT 'PurchaseOrderItems', COUNT(*) FROM [PurchaseOrderItems];
GO
