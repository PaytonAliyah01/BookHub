-- Create DiscussionPosts table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DiscussionPosts]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DiscussionPosts] (
        [PostId] INT IDENTITY(1,1) PRIMARY KEY,
        [ClubId] INT NOT NULL,
        [UserId] INT NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Content] NVARCHAR(MAX) NOT NULL,
        [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [UpdatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [IsSticky] BIT NOT NULL DEFAULT 0,
        [ReplyCount] INT NOT NULL DEFAULT 0,
        CONSTRAINT FK_DiscussionPosts_BookClubs FOREIGN KEY ([ClubId]) REFERENCES [dbo].[BookClubs]([ClubId]) ON DELETE CASCADE,
        CONSTRAINT FK_DiscussionPosts_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([UserId]) ON DELETE NO ACTION
    );
    
    CREATE INDEX IX_DiscussionPosts_ClubId ON [dbo].[DiscussionPosts]([ClubId]);
    CREATE INDEX IX_DiscussionPosts_UserId ON [dbo].[DiscussionPosts]([UserId]);
    CREATE INDEX IX_DiscussionPosts_CreatedDate ON [dbo].[DiscussionPosts]([CreatedDate] DESC);
END
GO

-- Create DiscussionReplies table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DiscussionReplies]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DiscussionReplies] (
        [ReplyId] INT IDENTITY(1,1) PRIMARY KEY,
        [PostId] INT NOT NULL,
        [UserId] INT NOT NULL,
        [Content] NVARCHAR(MAX) NOT NULL,
        [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [UpdatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_DiscussionReplies_DiscussionPosts FOREIGN KEY ([PostId]) REFERENCES [dbo].[DiscussionPosts]([PostId]) ON DELETE CASCADE,
        CONSTRAINT FK_DiscussionReplies_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([UserId]) ON DELETE NO ACTION
    );
    
    CREATE INDEX IX_DiscussionReplies_PostId ON [dbo].[DiscussionReplies]([PostId]);
    CREATE INDEX IX_DiscussionReplies_UserId ON [dbo].[DiscussionReplies]([UserId]);
    CREATE INDEX IX_DiscussionReplies_CreatedDate ON [dbo].[DiscussionReplies]([CreatedDate] DESC);
END
GO

-- Create PostAttachments table (optional - if you want to support attachments)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PostAttachments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PostAttachments] (
        [AttachmentId] INT IDENTITY(1,1) PRIMARY KEY,
        [PostId] INT NULL,
        [ReplyId] INT NULL,
        [FileName] NVARCHAR(255) NOT NULL,
        [FilePath] NVARCHAR(500) NOT NULL,
        [FileSize] BIGINT NOT NULL,
        [ContentType] NVARCHAR(100) NOT NULL,
        [UploadedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_PostAttachments_DiscussionPosts FOREIGN KEY ([PostId]) REFERENCES [dbo].[DiscussionPosts]([PostId]) ON DELETE CASCADE,
        CONSTRAINT FK_PostAttachments_DiscussionReplies FOREIGN KEY ([ReplyId]) REFERENCES [dbo].[DiscussionReplies]([ReplyId]) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_PostAttachments_PostId ON [dbo].[PostAttachments]([PostId]);
    CREATE INDEX IX_PostAttachments_ReplyId ON [dbo].[PostAttachments]([ReplyId]);
END
GO

PRINT 'Forum tables created successfully!'
