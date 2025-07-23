-- Use the correct database
USE SpendSmartNew;
GO

-- Check if columns exist and add them if they don't
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Categories]') AND name = 'CategoryName')
BEGIN
    ALTER TABLE [dbo].[Categories] ADD [CategoryName] nvarchar(100) NOT NULL DEFAULT 'Miscellaneous';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Categories]') AND name = 'Type')
BEGIN
    ALTER TABLE [dbo].[Categories] ADD [Type] nvarchar(max) NOT NULL DEFAULT 'Expense';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Categories]') AND name = 'Icon')
BEGIN
    ALTER TABLE [dbo].[Categories] ADD [Icon] nvarchar(max) NOT NULL DEFAULT '📦';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Categories]') AND name = 'Color')
BEGIN
    ALTER TABLE [dbo].[Categories] ADD [Color] nvarchar(max) NOT NULL DEFAULT '#9E9E9E';
END

PRINT 'Columns added successfully'; 