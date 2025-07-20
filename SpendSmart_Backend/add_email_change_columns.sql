-- Script to add email change tracking columns to Users table if they don't exist

-- Check if PendingEmail column exists, if not add it
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'PendingEmail')
BEGIN
    ALTER TABLE [Users] ADD [PendingEmail] nvarchar(max) NULL;
    PRINT 'Added PendingEmail column';
END
ELSE
BEGIN
    PRINT 'PendingEmail column already exists';
END

-- Check if EmailChangeToken column exists, if not add it
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'EmailChangeToken')
BEGIN
    ALTER TABLE [Users] ADD [EmailChangeToken] nvarchar(max) NULL;
    PRINT 'Added EmailChangeToken column';
END
ELSE
BEGIN
    PRINT 'EmailChangeToken column already exists';
END

-- Check if EmailChangeTokenExpiry column exists, if not add it
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'EmailChangeTokenExpiry')
BEGIN
    ALTER TABLE [Users] ADD [EmailChangeTokenExpiry] datetime2 NULL;
    PRINT 'Added EmailChangeTokenExpiry column';
END
ELSE
BEGIN
    PRINT 'EmailChangeTokenExpiry column already exists';
END

PRINT 'Email change tracking columns setup complete';
