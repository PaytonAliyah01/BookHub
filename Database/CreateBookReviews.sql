-- Create BookReviews table
CREATE TABLE BookReviews (
    ReviewId INT IDENTITY(1,1) PRIMARY KEY,
    BookId INT NOT NULL,
    UserId INT NOT NULL,
    Rating INT NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    ReviewTitle NVARCHAR(200) NOT NULL,
    ReviewText NVARCHAR(MAX) NOT NULL,
    ReviewDate DATETIME NOT NULL DEFAULT GETDATE(),
    LastModified DATETIME NULL,
    IsRecommended BIT NOT NULL DEFAULT 0,
    
    -- Create foreign key constraint to Books table
    CONSTRAINT FK_BookReviews_Books FOREIGN KEY (BookId) REFERENCES Books(BookId) ON DELETE CASCADE,
    
    -- Ensure one review per user per book
    CONSTRAINT UQ_BookReviews_User_Book UNIQUE (BookId, UserId)
);

-- Create indexes for better performance
CREATE INDEX IX_BookReviews_BookId ON BookReviews(BookId);
CREATE INDEX IX_BookReviews_UserId ON BookReviews(UserId);
CREATE INDEX IX_BookReviews_Rating ON BookReviews(Rating);
CREATE INDEX IX_BookReviews_ReviewDate ON BookReviews(ReviewDate);

-- Sample data (optional)
-- You can uncomment and modify these to add some test reviews
/*
INSERT INTO BookReviews (BookId, UserId, Rating, ReviewTitle, ReviewText, IsRecommended) VALUES
(1, 1, 5, 'Amazing Book!', 'This book completely changed my perspective. The writing is beautiful and the story is captivating from start to finish.', 1),
(1, 2, 4, 'Great Read', 'Really enjoyed this book. Some parts were a bit slow but overall very good.', 1),
(2, 1, 3, 'Okay Book', 'It was fine, nothing special but not bad either.', 0),
(3, 2, 5, 'Must Read!', 'Absolutely incredible! Could not put it down.', 1);
*/