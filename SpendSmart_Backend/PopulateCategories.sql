-- Use the correct database
USE SpendSmartNew;
GO

-- Update existing categories
UPDATE Categories SET CategoryName = 'Salary/Income', Type = 'Income', Icon = N'💵', Color = '#4CAF50' WHERE Id = 1;

-- Check and insert missing categories
IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Savings & Emergency Fund')
    INSERT INTO Categories (CategoryName, Type, Icon, Color) VALUES ('Savings & Emergency Fund', 'Expense', N'💰', '#2196F3');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Bills & Utilities')
    INSERT INTO Categories (CategoryName, Type, Icon, Color) VALUES ('Bills & Utilities', 'Expense', N'💡', '#FF9800');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Food & Beverages')
    INSERT INTO Categories (CategoryName, Type, Icon, Color) VALUES ('Food & Beverages', 'Expense', N'🍔', '#FF5722');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Transportation')
    INSERT INTO Categories (CategoryName, Type, Icon, Color) VALUES ('Transportation', 'Expense', N'🚗', '#03A9F4');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Housing & Rent')
    INSERT INTO Categories (CategoryName, Type, Icon, Color) VALUES ('Housing & Rent', 'Expense', N'🏠', '#795548');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Shopping')
    INSERT INTO Categories (CategoryName, Type, Icon, Color) VALUES ('Shopping', 'Expense', N'🛍️', '#E91E63');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Healthcare')
    INSERT INTO Categories (CategoryName, Type, Icon, Color) VALUES ('Healthcare', 'Expense', N'🏥', '#F44336');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Debt / Loan Payments')
    INSERT INTO Categories (CategoryName, Type, Icon, Color) VALUES ('Debt / Loan Payments', 'Expense', N'💳', '#9C27B0');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Taxes')
    INSERT INTO Categories (CategoryName, Type, Icon, Color) VALUES ('Taxes', 'Expense', N'💸', '#607D8B');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Insurance')
    INSERT INTO Categories (CategoryName, Type, Icon, Color) VALUES ('Insurance', 'Expense', N'🛡️', '#3F51B5');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Education')
    INSERT INTO Categories (CategoryName, Type, Icon, Color) VALUES ('Education', 'Expense', N'🎓', '#009688');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Entertainment')
    INSERT INTO Categories (CategoryName, Type, Icon, Color) VALUES ('Entertainment', 'Expense', N'🎬', '#FF6F00');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Subscriptions')
    INSERT INTO Categories (CategoryName, Type, Icon, Color) VALUES ('Subscriptions', 'Expense', N'📱', '#6A1B9A');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Clothing & Accessories')
    INSERT INTO Categories (CategoryName, Type, Icon, Color) VALUES ('Clothing & Accessories', 'Expense', N'👔', '#8BC34A');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Personal Care')
    INSERT INTO Categories (CategoryName, Type, Icon, Color) VALUES ('Personal Care', 'Expense', N'💄', '#E1BEE7');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Travel')
    INSERT INTO Categories (CategoryName, Type, Icon, Color) VALUES ('Travel', 'Expense', N'✈️', '#00BCD4');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Gifts & Donations')
    INSERT INTO Categories (CategoryName, Type, Icon, Color) VALUES ('Gifts & Donations', 'Expense', N'🎁', '#CDDC39');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Events & Celebrations')
    INSERT INTO Categories (CategoryName, Type, Icon, Color) VALUES ('Events & Celebrations', 'Expense', N'🎉', '#FFC107');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Pets')
    INSERT INTO Categories (CategoryName, Type, Icon, Color) VALUES ('Pets', 'Expense', N'🐶', '#FF8A65');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Maintenance & Repairs')
    INSERT INTO Categories (CategoryName, Type, Icon, Color) VALUES ('Maintenance & Repairs', 'Expense', N'🛠️', '#78909C');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Business')
    INSERT INTO Categories (CategoryName, Type, Icon, Color) VALUES ('Business', 'Expense', N'🏢', '#1976D2');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Investment')
    INSERT INTO Categories (CategoryName, Type, Icon, Color) VALUES ('Investment', 'Income', N'📈', '#388E3C');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Interest & Dividends')
    INSERT INTO Categories (CategoryName, Type, Icon, Color) VALUES ('Interest & Dividends', 'Income', N'💹', '#00796B');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Bank Charges & Fees')
    INSERT INTO Categories (CategoryName, Type, Icon, Color) VALUES ('Bank Charges & Fees', 'Expense', N'🏦', '#5D4037');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Legal & Professional Services')
    INSERT INTO Categories (CategoryName, Type, Icon, Color) VALUES ('Legal & Professional Services', 'Expense', N'⚖️', '#424242');

IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryName = 'Other / Miscellaneous')
    INSERT INTO Categories (CategoryName, Type, Icon, Color) VALUES ('Other / Miscellaneous', 'Expense', N'📦', '#9E9E9E');

PRINT 'Categories updated and inserted successfully'; 