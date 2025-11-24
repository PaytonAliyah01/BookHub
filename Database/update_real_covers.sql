-- Update book covers to use the most reliable Open Library approach
-- First try with ISBN, then title, then author as fallbacks

-- For books with valid ISBNs, use ISBN-based covers (most reliable)
UPDATE Books 
SET CoverUrl = 'https://covers.openlibrary.org/b/isbn/' + REPLACE(REPLACE(REPLACE(ISBN, '-', ''), ' ', ''), 'X', 'x') + '-L.jpg'
WHERE ISBN IS NOT NULL 
  AND ISBN != '' 
  AND LEN(REPLACE(REPLACE(ISBN, '-', ''), ' ', '')) >= 10;

-- For books without valid ISBNs, use title-based covers
UPDATE Books 
SET CoverUrl = 'https://covers.openlibrary.org/b/title/' + REPLACE(LOWER(Title), ' ', '+') + '-L.jpg'
WHERE (ISBN IS NULL OR ISBN = '' OR LEN(REPLACE(REPLACE(ISBN, '-', ''), ' ', '')) < 10);

-- Add some specific well-known books with their correct ISBNs for better matching
UPDATE Books SET ISBN = '9780007448036', CoverUrl = 'https://covers.openlibrary.org/b/isbn/9780007448036-L.jpg' WHERE Title LIKE '%Game of Thrones%';
UPDATE Books SET ISBN = '9780547928227', CoverUrl = 'https://covers.openlibrary.org/b/isbn/9780547928227-L.jpg' WHERE Title LIKE '%Hobbit%';
UPDATE Books SET ISBN = '9780439708180', CoverUrl = 'https://covers.openlibrary.org/b/isbn/9780439708180-L.jpg' WHERE Title LIKE '%Harry Potter and the Sorcerer%';
UPDATE Books SET ISBN = '9780439064873', CoverUrl = 'https://covers.openlibrary.org/b/isbn/9780439064873-L.jpg' WHERE Title LIKE '%Harry Potter and the Chamber%';
UPDATE Books SET ISBN = '9780316015844', CoverUrl = 'https://covers.openlibrary.org/b/isbn/9780316015844-L.jpg' WHERE Title LIKE '%Twilight%' AND Author LIKE '%Meyer%';
UPDATE Books SET ISBN = '9780061120084', CoverUrl = 'https://covers.openlibrary.org/b/isbn/9780061120084-L.jpg' WHERE Title LIKE '%To Kill a Mockingbird%';
UPDATE Books SET ISBN = '9780141439518', CoverUrl = 'https://covers.openlibrary.org/b/isbn/9780141439518-L.jpg' WHERE Title LIKE '%Great Gatsby%';
UPDATE Books SET ISBN = '9780451524935', CoverUrl = 'https://covers.openlibrary.org/b/isbn/9780451524935-L.jpg' WHERE Title LIKE '%1984%';
UPDATE Books SET ISBN = '9780547773698', CoverUrl = 'https://covers.openlibrary.org/b/isbn/9780547773698-L.jpg' WHERE Title LIKE '%Hunger Games%' AND NOT Title LIKE '%Catching%';
UPDATE Books SET ISBN = '9780062315007', CoverUrl = 'https://covers.openlibrary.org/b/isbn/9780062315007-L.jpg' WHERE Title LIKE '%Alchemist%' AND Author LIKE '%Coelho%';

PRINT 'Updated book covers with Open Library URLs using ISBNs and titles';
PRINT 'Real book covers will now be fetched from Open Library database';