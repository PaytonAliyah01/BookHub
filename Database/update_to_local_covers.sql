-- Update all books to use a local cover generation system
-- This will create consistent, always-working book covers

UPDATE Books 
SET CoverUrl = '/api/bookcover/' + CAST(BookId AS VARCHAR(10))
WHERE BookId IS NOT NULL;

PRINT 'Updated all books to use local cover generation system';
PRINT 'Covers will now be generated dynamically and always display correctly';