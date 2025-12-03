-- Add reading tracking columns to UserBooks table
-- This script adds support for reading dates and progress tracking

-- Check if columns don't exist before adding them
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'UserBooks' AND COLUMN_NAME = 'DateStarted')
BEGIN
    ALTER TABLE UserBooks ADD DateStarted DATETIME NULL;
    PRINT 'Added DateStarted column to UserBooks table.';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'UserBooks' AND COLUMN_NAME = 'DateFinished')
BEGIN
    ALTER TABLE UserBooks ADD DateFinished DATETIME NULL;
    PRINT 'Added DateFinished column to UserBooks table.';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'UserBooks' AND COLUMN_NAME = 'CurrentPage')
BEGIN
    ALTER TABLE UserBooks ADD CurrentPage INT NULL;
    PRINT 'Added CurrentPage column to UserBooks table.';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'UserBooks' AND COLUMN_NAME = 'ReadingProgress')
BEGIN
    ALTER TABLE UserBooks ADD ReadingProgress DECIMAL(5,2) NULL;
    PRINT 'Added ReadingProgress column to UserBooks table.';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'UserBooks' AND COLUMN_NAME = 'TotalPages')
BEGIN
    ALTER TABLE UserBooks ADD TotalPages INT NULL;
    PRINT 'Added TotalPages column to UserBooks table.';
END

-- Update existing "Reading" books to have DateStarted as DateAdded if not already set
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'UserBooks' AND COLUMN_NAME = 'DateStarted')
BEGIN
    UPDATE UserBooks 
    SET DateStarted = DateAdded 
    WHERE Status = 'Reading' 
    AND DateStarted IS NULL;
    PRINT 'Updated existing Reading books with start dates.';
END

-- Update existing "Read" books to have DateFinished as DateAdded if not already set  
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'UserBooks' AND COLUMN_NAME = 'DateFinished')
BEGIN
    UPDATE UserBooks 
    SET DateFinished = DateAdded 
    WHERE Status = 'Read' 
    AND DateFinished IS NULL;
    PRINT 'Updated existing Read books with finish dates.';
END

PRINT 'Reading tracking columns migration completed successfully.';