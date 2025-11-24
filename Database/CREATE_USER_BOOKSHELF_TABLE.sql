-- SQL Script to create UserBookshelf table for user-specific book collections
-- Run this in your SQL Server Management Studio or Azure Data Studio

-- Create UserBookshelf table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserBookshelf' AND xtype='U')
BEGIN
    CREATE TABLE UserBookshelf (
        UserBookshelfId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        BookId INT NOT NULL,
        DateAdded DATETIME2 DEFAULT GETDATE(),
        Status NVARCHAR(50) DEFAULT 'Want to Read', -- 'Want to Read', 'Reading', 'Read'
        Rating INT NULL, -- 1-5 stars, nullable
        Notes NVARCHAR(1000) DEFAULT '',
        
        -- Foreign key constraints
        FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
        FOREIGN KEY (BookId) REFERENCES Books(BookId) ON DELETE CASCADE,
        
        -- Unique constraint to prevent duplicate entries
        UNIQUE(UserId, BookId)
    );
    
    PRINT 'UserBookshelf table created successfully.';
END
ELSE
BEGIN
    PRINT 'UserBookshelf table already exists.';
END

-- Create indexes for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserBookshelf_UserId')
BEGIN
    CREATE INDEX IX_UserBookshelf_UserId ON UserBookshelf(UserId);
    PRINT 'Index on UserId created.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserBookshelf_BookId')
BEGIN
    CREATE INDEX IX_UserBookshelf_BookId ON UserBookshelf(BookId);
    PRINT 'Index on BookId created.';
END

-- Verify the table structure
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    CHARACTER_MAXIMUM_LENGTH, 
    IS_NULLABLE, 
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'UserBookshelf'
ORDER BY ORDINAL_POSITION;