-- Simple script to add OwnershipType column manually if needed
-- Run this in SQL Server Management Studio or any SQL client connected to BookHubDb

-- Add OwnershipType column if it doesn't exist
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'UserBooks' AND COLUMN_NAME = 'OwnershipType'
)
BEGIN
    ALTER TABLE UserBooks ADD OwnershipType NVARCHAR(50) NOT NULL DEFAULT 'Physical'
    PRINT 'OwnershipType column added successfully'
END
ELSE
BEGIN
    PRINT 'OwnershipType column already exists'
END

-- Update existing records
UPDATE UserBooks 
SET OwnershipType = CASE 
    WHEN IsOwned = 1 THEN 'Physical'
    ELSE 'Wishlist'
END
WHERE OwnershipType = 'Physical' -- Only update default values

PRINT 'Existing records updated with ownership types'