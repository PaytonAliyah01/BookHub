-- Add Username, DateOfBirth, and Sex fields to Users table

-- Add Username column if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'Username')
BEGIN
    ALTER TABLE Users ADD Username NVARCHAR(50) NULL;
    PRINT 'Username column added successfully!';
END
ELSE
BEGIN
    PRINT 'Username column already exists.';
END

-- Add DateOfBirth column if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'DateOfBirth')
BEGIN
    ALTER TABLE Users ADD DateOfBirth DATE NULL;
    PRINT 'DateOfBirth column added successfully!';
END
ELSE
BEGIN
    PRINT 'DateOfBirth column already exists.';
END

-- Add Sex column if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'Sex')
BEGIN
    ALTER TABLE Users ADD Sex NVARCHAR(20) NULL;
    PRINT 'Sex column added successfully!';
END
ELSE
BEGIN
    PRINT 'Sex column already exists.';
END

-- Create unique index on Username if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Username' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE UNIQUE INDEX IX_Users_Username ON Users(Username) WHERE Username IS NOT NULL;
    PRINT 'Unique index on Username created successfully!';
END
ELSE
BEGIN
    PRINT 'Unique index on Username already exists.';
END

PRINT 'User fields update completed!';
