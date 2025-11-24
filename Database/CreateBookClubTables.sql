-- Create BookClubs table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='BookClubs' AND xtype='U')
BEGIN
    CREATE TABLE BookClubs (
        BookClubId INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500),
        CreatorUserId INT NOT NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        CurrentBookId INT NULL,
        CurrentBookStartDate DATETIME NULL,
        CurrentBookEndDate DATETIME NULL,
        IsPrivate BIT NOT NULL DEFAULT 0,
        MaxMembers INT NOT NULL DEFAULT 50,
        MeetingSchedule NVARCHAR(200) NULL,
        Genre NVARCHAR(100) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        FOREIGN KEY (CreatorUserId) REFERENCES Users(UserId),
        FOREIGN KEY (CurrentBookId) REFERENCES Books(BookId)
    );
    
    CREATE INDEX IX_BookClubs_CreatorUserId ON BookClubs(CreatorUserId);
    CREATE INDEX IX_BookClubs_IsActive ON BookClubs(IsActive);
    CREATE INDEX IX_BookClubs_Genre ON BookClubs(Genre);
END

-- Create BookClubMembers table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='BookClubMembers' AND xtype='U')
BEGIN
    CREATE TABLE BookClubMembers (
        BookClubMemberId INT IDENTITY(1,1) PRIMARY KEY,
        BookClubId INT NOT NULL,
        UserId INT NOT NULL,
        JoinedDate DATETIME NOT NULL DEFAULT GETDATE(),
        Role NVARCHAR(20) NOT NULL DEFAULT 'Member',
        IsActive BIT NOT NULL DEFAULT 1,
        FOREIGN KEY (BookClubId) REFERENCES BookClubs(BookClubId) ON DELETE CASCADE,
        FOREIGN KEY (UserId) REFERENCES Users(UserId),
        CONSTRAINT UQ_BookClubMembers UNIQUE (BookClubId, UserId),
        CONSTRAINT CK_BookClubMembers_Role CHECK (Role IN ('Member', 'Moderator', 'Creator'))
    );
    
    CREATE INDEX IX_BookClubMembers_BookClubId ON BookClubMembers(BookClubId);
    CREATE INDEX IX_BookClubMembers_UserId ON BookClubMembers(UserId);
    CREATE INDEX IX_BookClubMembers_IsActive ON BookClubMembers(IsActive);
END

-- Create BookClubDiscussions table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='BookClubDiscussions' AND xtype='U')
BEGIN
    CREATE TABLE BookClubDiscussions (
        DiscussionId INT IDENTITY(1,1) PRIMARY KEY,
        BookClubId INT NOT NULL,
        UserId INT NOT NULL,
        Message NVARCHAR(MAX) NOT NULL,
        PostedDate DATETIME NOT NULL DEFAULT GETDATE(),
        ReplyToId INT NULL,
        FOREIGN KEY (BookClubId) REFERENCES BookClubs(BookClubId) ON DELETE CASCADE,
        FOREIGN KEY (UserId) REFERENCES Users(UserId),
        FOREIGN KEY (ReplyToId) REFERENCES BookClubDiscussions(DiscussionId)
    );
    
    CREATE INDEX IX_BookClubDiscussions_BookClubId ON BookClubDiscussions(BookClubId);
    CREATE INDEX IX_BookClubDiscussions_UserId ON BookClubDiscussions(UserId);
    CREATE INDEX IX_BookClubDiscussions_PostedDate ON BookClubDiscussions(PostedDate);
END

PRINT 'BookClub system tables created successfully!';