-- Create Friends table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Friends' AND xtype='U')
BEGIN
    CREATE TABLE Friends (
        FriendshipId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        FriendUserId INT NOT NULL,
        FriendsSince DATETIME NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (UserId) REFERENCES Users(UserId),
        FOREIGN KEY (FriendUserId) REFERENCES Users(UserId),
        CONSTRAINT UQ_Friends UNIQUE (UserId, FriendUserId),
        CONSTRAINT CK_Friends_NotSelf CHECK (UserId != FriendUserId)
    );
    
    CREATE INDEX IX_Friends_UserId ON Friends(UserId);
    CREATE INDEX IX_Friends_FriendUserId ON Friends(FriendUserId);
END

-- Create FriendRequests table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FriendRequests' AND xtype='U')
BEGIN
    CREATE TABLE FriendRequests (
        RequestId INT IDENTITY(1,1) PRIMARY KEY,
        FromUserId INT NOT NULL,
        ToUserId INT NOT NULL,
        RequestDate DATETIME NOT NULL DEFAULT GETDATE(),
        Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        ResponseDate DATETIME NULL,
        FOREIGN KEY (FromUserId) REFERENCES Users(UserId),
        FOREIGN KEY (ToUserId) REFERENCES Users(UserId),
        CONSTRAINT CK_FriendRequests_Status CHECK (Status IN ('Pending', 'Accepted', 'Declined')),
        CONSTRAINT CK_FriendRequests_NotSelf CHECK (FromUserId != ToUserId)
    );
    
    CREATE INDEX IX_FriendRequests_FromUserId ON FriendRequests(FromUserId);
    CREATE INDEX IX_FriendRequests_ToUserId ON FriendRequests(ToUserId);
    CREATE INDEX IX_FriendRequests_Status ON FriendRequests(Status);
END

PRINT 'Friends system tables created successfully!';