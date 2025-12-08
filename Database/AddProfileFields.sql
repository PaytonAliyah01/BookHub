-- =============================================
-- Add New Profile Fields to Users and UserBookshelf Tables
-- Date: December 8, 2025
-- =============================================

USE BookHubDB;
GO

-- Add new columns to Users table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Location')
BEGIN
    ALTER TABLE Users ADD Location NVARCHAR(100) NULL;
    PRINT 'Added Location column to Users table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'FavoriteGenres')
BEGIN
    ALTER TABLE Users ADD FavoriteGenres NVARCHAR(255) NULL;
    PRINT 'Added FavoriteGenres column to Users table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'FavoriteAuthors')
BEGIN
    ALTER TABLE Users ADD FavoriteAuthors NVARCHAR(255) NULL;
    PRINT 'Added FavoriteAuthors column to Users table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'PreferredFormat')
BEGIN
    ALTER TABLE Users ADD PreferredFormat NVARCHAR(20) NULL;
    PRINT 'Added PreferredFormat column to Users table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'FavoriteQuote')
BEGIN
    ALTER TABLE Users ADD FavoriteQuote NVARCHAR(500) NULL;
    PRINT 'Added FavoriteQuote column to Users table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'DateJoined')
BEGIN
    ALTER TABLE Users ADD DateJoined DATETIME NOT NULL DEFAULT GETDATE();
    PRINT 'Added DateJoined column to Users table';
END

-- Add IsFavorite column to UserBookshelf (or UserBooks depending on your table name)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'UserBookshelf')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('UserBookshelf') AND name = 'IsFavorite')
    BEGIN
        ALTER TABLE UserBookshelf ADD IsFavorite BIT NOT NULL DEFAULT 0;
        PRINT 'Added IsFavorite column to UserBookshelf table';
    END
END

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'UserBooks')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('UserBooks') AND name = 'IsFavorite')
    BEGIN
        ALTER TABLE UserBooks ADD IsFavorite BIT NOT NULL DEFAULT 0;
        PRINT 'Added IsFavorite column to UserBooks table';
    END
END

-- Create Achievements table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Achievements')
BEGIN
    CREATE TABLE Achievements (
        AchievementId INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(255) NOT NULL,
        BadgeIcon NVARCHAR(100) NULL,
        Category NVARCHAR(50) NOT NULL, -- 'Reading', 'Social', 'Reviews', 'BookClubs'
        Threshold INT NOT NULL, -- The number needed to unlock (e.g., 5 books, 10 reviews)
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
    );
    PRINT 'Created Achievements table';
END

-- Create UserAchievements junction table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserAchievements')
BEGIN
    CREATE TABLE UserAchievements (
        UserAchievementId INT PRIMARY KEY IDENTITY(1,1),
        UserId INT NOT NULL,
        AchievementId INT NOT NULL,
        UnlockedDate DATETIME NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
        FOREIGN KEY (AchievementId) REFERENCES Achievements(AchievementId) ON DELETE CASCADE,
        CONSTRAINT UQ_UserAchievement UNIQUE(UserId, AchievementId)
    );
    PRINT 'Created UserAchievements table';
END

-- Insert default achievements
IF NOT EXISTS (SELECT * FROM Achievements)
BEGIN
    INSERT INTO Achievements (Name, Description, BadgeIcon, Category, Threshold) VALUES
    ('First Book', 'Read your first book', 'book', 'Reading', 1),
    ('Bookworm', 'Read 5 books', 'book-heart', 'Reading', 5),
    ('Library Master', 'Read 10 books', 'book-fill', 'Reading', 10),
    ('Century Reader', 'Read 25 books', 'award', 'Reading', 25),
    ('Book Collector', 'Read 50 books', 'trophy', 'Reading', 50),
    ('Legendary Reader', 'Read 100 books', 'star-fill', 'Reading', 100),
    
    ('First Review', 'Write your first review', 'pencil', 'Reviews', 1),
    ('Reviewer', 'Write 5 reviews', 'pencil-square', 'Reviews', 5),
    ('Critic', 'Write 10 reviews', 'pen', 'Reviews', 10),
    ('Master Critic', 'Write 25 reviews', 'pen-fill', 'Reviews', 25),
    
    ('Social Butterfly', 'Join your first book club', 'people', 'Social', 1),
    ('Club Member', 'Join 3 book clubs', 'people-fill', 'Social', 3),
    ('Club Enthusiast', 'Join 5 book clubs', 'chat-dots', 'Social', 5),
    
    ('Conversationalist', 'Start 5 discussions', 'chat-left-text', 'Social', 5),
    ('Discussion Leader', 'Start 10 discussions', 'chat-quote', 'Social', 10),
    
    ('Friendship Started', 'Make your first friend', 'heart', 'Social', 1),
    ('Popular', 'Make 5 friends', 'heart-fill', 'Social', 5),
    ('Community Leader', 'Make 10 friends', 'stars', 'Social', 10);
    
    PRINT 'Inserted default achievements';
END

PRINT 'Migration completed successfully!';
GO
