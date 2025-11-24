-- Add OwnershipType column to UserBooks table
-- This script should be run against the BookHubDb database

-- Check if column exists before adding
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'UserBooks' AND COLUMN_NAME = 'OwnershipType')
BEGIN
    -- Add OwnershipType column
    ALTER TABLE UserBooks 
    ADD OwnershipType NVARCHAR(50) NOT NULL DEFAULT 'Physical'
    
    PRINT 'OwnershipType column added to UserBooks table'
END
ELSE
BEGIN
    PRINT 'OwnershipType column already exists in UserBooks table'
END

-- Update existing records based on IsOwned column
UPDATE UserBooks 
SET OwnershipType = CASE 
    WHEN IsOwned = 1 THEN 'Physical'
    ELSE 'Wishlist'
END
WHERE OwnershipType = 'Physical' -- Only update records with default value

PRINT 'Updated existing UserBooks records with appropriate OwnershipType values'