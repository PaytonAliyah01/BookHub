-- Sample data for testing the Friends system
-- Note: This assumes you have at least 3 users in your Users table

-- Insert some sample friend relationships (if users exist)
IF EXISTS (SELECT 1 FROM Users WHERE UserId = 1) AND EXISTS (SELECT 1 FROM Users WHERE UserId = 2)
BEGIN
    -- Create friendship between User 1 and User 2
    IF NOT EXISTS (SELECT 1 FROM Friends WHERE (UserId = 1 AND FriendUserId = 2) OR (UserId = 2 AND FriendUserId = 1))
    BEGIN
        INSERT INTO Friends (UserId, FriendUserId, FriendsSince) VALUES (1, 2, GETDATE());
        INSERT INTO Friends (UserId, FriendUserId, FriendsSince) VALUES (2, 1, GETDATE());
    END
END

-- Insert some sample friend requests
IF EXISTS (SELECT 1 FROM Users WHERE UserId = 3) AND EXISTS (SELECT 1 FROM Users WHERE UserId = 1)
BEGIN
    -- User 3 sends request to User 1 (if not already friends or request exists)
    IF NOT EXISTS (SELECT 1 FROM FriendRequests WHERE (FromUserId = 3 AND ToUserId = 1) OR (FromUserId = 1 AND ToUserId = 3))
       AND NOT EXISTS (SELECT 1 FROM Friends WHERE (UserId = 1 AND FriendUserId = 3) OR (UserId = 3 AND FriendUserId = 1))
    BEGIN
        INSERT INTO FriendRequests (FromUserId, ToUserId, RequestDate, Status) 
        VALUES (3, 1, GETDATE(), 'Pending');
    END
END

PRINT 'Sample friends data inserted successfully!';