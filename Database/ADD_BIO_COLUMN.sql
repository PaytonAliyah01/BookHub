-- SQL Script to add Bio column to Users table
-- Run this in your SQL Server Management Studio or Azure Data Studio

-- Check if the Bio column exists before adding it
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'Bio'
)
BEGIN
    ALTER TABLE Users 
    ADD Bio NVARCHAR(500) DEFAULT '';
    
    PRINT 'Bio column added successfully to Users table.';
END
ELSE
BEGIN
    PRINT 'Bio column already exists in Users table.';
END

-- Verify the table structure
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    CHARACTER_MAXIMUM_LENGTH, 
    IS_NULLABLE, 
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Users'
ORDER BY ORDINAL_POSITION;