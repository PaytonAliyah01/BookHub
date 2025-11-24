using BookHub.DAL;

namespace BookHub.BLL
{
    public class FriendBLL
    {
        private readonly FriendDAL _friendDAL;

        public FriendBLL(string connectionString)
        {
            _friendDAL = new FriendDAL(connectionString);
        }

        public FriendBLL(FriendDAL friendDAL)
        {
            _friendDAL = friendDAL;
        }

        // Search for users with validation
        public List<User> SearchUsers(string searchTerm, int currentUserId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return new List<User>();

                if (searchTerm.Length < 2)
                    throw new ArgumentException("Search term must be at least 2 characters long.");

                if (currentUserId <= 0)
                    throw new ArgumentException("Invalid user ID.");

                return _friendDAL.SearchUsers(searchTerm.Trim(), currentUserId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching for users: {ex.Message}", ex);
            }
        }

        // Send friend request with validation
        public bool SendFriendRequest(int fromUserId, int toUserId)
        {
            try
            {
                if (fromUserId <= 0 || toUserId <= 0)
                    throw new ArgumentException("Invalid user IDs provided.");

                if (fromUserId == toUserId)
                    throw new ArgumentException("Cannot send friend request to yourself.");

                return _friendDAL.SendFriendRequest(fromUserId, toUserId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error sending friend request: {ex.Message}", ex);
            }
        }

        // Get pending friend requests
        public List<FriendRequest> GetPendingRequests(int userId)
        {
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("Invalid user ID.");

                return _friendDAL.GetPendingRequests(userId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting pending requests: {ex.Message}", ex);
            }
        }

        // Accept friend request with validation
        public bool AcceptFriendRequest(int requestId, int userId)
        {
            try
            {
                if (requestId <= 0 || userId <= 0)
                    throw new ArgumentException("Invalid request ID or user ID.");

                return _friendDAL.AcceptFriendRequest(requestId, userId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error accepting friend request: {ex.Message}", ex);
            }
        }

        // Decline friend request with validation
        public bool DeclineFriendRequest(int requestId, int userId)
        {
            try
            {
                if (requestId <= 0 || userId <= 0)
                    throw new ArgumentException("Invalid request ID or user ID.");

                return _friendDAL.DeclineFriendRequest(requestId, userId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error declining friend request: {ex.Message}", ex);
            }
        }

        // Get user's friends
        public List<User> GetFriends(int userId)
        {
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("Invalid user ID.");

                return _friendDAL.GetFriends(userId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting friends list: {ex.Message}", ex);
            }
        }

        // Get friends' reading activity
        public List<dynamic> GetFriendsActivity(int userId, int limit = 20)
        {
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("Invalid user ID.");

                var activities = _friendDAL.GetFriendsActivity(userId);
                return activities.Take(limit).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting friends activity: {ex.Message}", ex);
            }
        }

        // Remove friend with validation
        public bool RemoveFriend(int userId, int friendUserId)
        {
            try
            {
                if (userId <= 0 || friendUserId <= 0)
                    throw new ArgumentException("Invalid user IDs provided.");

                if (userId == friendUserId)
                    throw new ArgumentException("Cannot remove yourself as a friend.");

                return _friendDAL.RemoveFriend(userId, friendUserId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error removing friend: {ex.Message}", ex);
            }
        }

        // Check if users are friends
        public bool AreFriends(int userId1, int userId2)
        {
            try
            {
                if (userId1 <= 0 || userId2 <= 0)
                    return false;

                return _friendDAL.AreFriends(userId1, userId2);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error checking friendship status: {ex.Message}", ex);
            }
        }

        // Check if friend request exists
        public bool FriendRequestExists(int fromUserId, int toUserId)
        {
            try
            {
                if (fromUserId <= 0 || toUserId <= 0)
                    return false;

                return _friendDAL.FriendRequestExists(fromUserId, toUserId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error checking friend request status: {ex.Message}", ex);
            }
        }

        // Get relationship status between two users
        public string GetRelationshipStatus(int currentUserId, int otherUserId)
        {
            try
            {
                if (currentUserId <= 0 || otherUserId <= 0)
                    return "Unknown";

                if (currentUserId == otherUserId)
                    return "Self";

                if (AreFriends(currentUserId, otherUserId))
                    return "Friends";

                if (FriendRequestExists(currentUserId, otherUserId))
                    return "RequestPending";

                if (FriendRequestExists(otherUserId, currentUserId))
                    return "RequestReceived";

                return "NotFriends";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting relationship status: {ex.Message}", ex);
            }
        }
    }
}