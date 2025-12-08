-- Add missing columns to BookClubs table if they don't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BookClubs' AND COLUMN_NAME = 'CreatedDate')
BEGIN
    ALTER TABLE BookClubs ADD CreatedDate DATETIME NOT NULL DEFAULT GETDATE();
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BookClubs' AND COLUMN_NAME = 'CurrentBookId')
BEGIN
    ALTER TABLE BookClubs ADD CurrentBookId INT NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BookClubs' AND COLUMN_NAME = 'CurrentBookStartDate')
BEGIN
    ALTER TABLE BookClubs ADD CurrentBookStartDate DATETIME NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BookClubs' AND COLUMN_NAME = 'CurrentBookEndDate')
BEGIN
    ALTER TABLE BookClubs ADD CurrentBookEndDate DATETIME NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BookClubs' AND COLUMN_NAME = 'IsPrivate')
BEGIN
    ALTER TABLE BookClubs ADD IsPrivate BIT NOT NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BookClubs' AND COLUMN_NAME = 'MaxMembers')
BEGIN
    ALTER TABLE BookClubs ADD MaxMembers INT NOT NULL DEFAULT 50;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BookClubs' AND COLUMN_NAME = 'MeetingSchedule')
BEGIN
    ALTER TABLE BookClubs ADD MeetingSchedule NVARCHAR(200) NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BookClubs' AND COLUMN_NAME = 'Genre')
BEGIN
    ALTER TABLE BookClubs ADD Genre NVARCHAR(100) NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'BookClubs' AND COLUMN_NAME = 'IsActive')
BEGIN
    ALTER TABLE BookClubs ADD IsActive BIT NOT NULL DEFAULT 1;
END

-- Create BookClubDiscussions table for the Forum
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='BookClubDiscussions' AND xtype='U')
BEGIN
    CREATE TABLE BookClubDiscussions (
        DiscussionId INT IDENTITY(1,1) PRIMARY KEY,
        BookClubId INT NOT NULL,
        UserId INT NOT NULL,
        Message NVARCHAR(MAX) NOT NULL,
        PostedDate DATETIME NOT NULL DEFAULT GETDATE(),
        ReplyToId INT NULL,
        FOREIGN KEY (BookClubId) REFERENCES BookClubs(ClubId) ON DELETE CASCADE,
        FOREIGN KEY (UserId) REFERENCES Users(UserId),
        FOREIGN KEY (ReplyToId) REFERENCES BookClubDiscussions(DiscussionId)
    );
    
    CREATE INDEX IX_BookClubDiscussions_BookClubId ON BookClubDiscussions(BookClubId);
    CREATE INDEX IX_BookClubDiscussions_UserId ON BookClubDiscussions(UserId);
    CREATE INDEX IX_BookClubDiscussions_PostedDate ON BookClubDiscussions(PostedDate);
    CREATE INDEX IX_BookClubDiscussions_ReplyToId ON BookClubDiscussions(ReplyToId);
    
    PRINT 'BookClubDiscussions table created successfully!';
END
ELSE
BEGIN
    PRINT 'BookClubDiscussions table already exists.';
END

PRINT 'BookClub schema updated successfully!';
