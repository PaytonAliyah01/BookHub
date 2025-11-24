using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.BLL;
using BookHub.DAL;

namespace BookHub.Presentation.Pages
{
    public class BookClubsModel : PageModel
    {
        private readonly BookClubBLL _bookClubBLL;
        private readonly string _connectionString;

        public BookClubsModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            _bookClubBLL = new BookClubBLL(_connectionString);
        }

        [BindProperty]
        public BookClub NewBookClub { get; set; } = new BookClub();

        [BindProperty]
        public string SearchTerm { get; set; } = string.Empty;

        [BindProperty]
        public string SelectedGenre { get; set; } = string.Empty;

        public List<BookClub> BookClubs { get; set; } = new List<BookClub>();
        public List<BookClub> UserBookClubs { get; set; } = new List<BookClub>();
        public Dictionary<int, string> MembershipStatus { get; set; } = new Dictionary<int, string>();
        
        // Admin management properties
        public List<BookClubMember> ClubMembers { get; set; } = new List<BookClubMember>();
        public List<BookClubMember> PendingMembers { get; set; } = new List<BookClubMember>();
        public List<BookClubMember> CurrentMembers { get; set; } = new List<BookClubMember>();
        public Dictionary<int, bool> IsClubAdmin { get; set; } = new Dictionary<int, bool>();
        public string? CurrentUserId { get; set; }
        public int? SelectedClubId { get; set; }
        public BookClub? SelectedClub { get; set; }

        public string CurrentTab { get; set; } = "browse";

        public List<string> AvailableGenres { get; set; } = new List<string>
        {
            "Fiction", "Non-Fiction", "Mystery", "Romance", "Sci-Fi", "Fantasy", 
            "Biography", "History", "Self-Help", "Business", "Health", "Cooking",
            "Travel", "Art", "Religion", "Philosophy", "Poetry", "Drama"
        };

        public void OnGet(string tab = "browse", int? clubId = null)
        {
            CurrentTab = tab;
            CurrentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (CurrentTab == "manage" && clubId.HasValue)
            {
                LoadClubManagementData(clubId.Value);
            }
            
            LoadBookClubData();
        }

        public IActionResult OnPostSearch()
        {
            var userId = GetCurrentUserId();
            if (userId <= 0)
            {
                TempData["ErrorMessage"] = "You must be logged in to search book clubs.";
                return RedirectToPage("/Login");
            }

            try
            {
                BookClubs = _bookClubBLL.SearchBookClubs(SearchTerm, SelectedGenre);
                
                // Get membership status for each club
                foreach (var club in BookClubs)
                {
                    MembershipStatus[club.ClubId] = _bookClubBLL.GetMembershipStatus(club.ClubId, userId);
                }

                CurrentTab = "browse";
                LoadUserBookClubs();
                return Page();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                CurrentTab = "browse";
                LoadBookClubData();
                return Page();
            }
        }

        public IActionResult OnPostCreateClub()
        {
            var userId = GetCurrentUserId();
            if (userId <= 0)
            {
                TempData["ErrorMessage"] = "You must be logged in to create a book club.";
                return RedirectToPage("/Login");
            }

            try
            {
                // Validate the model
                var validationErrors = _bookClubBLL.ValidateBookClub(NewBookClub);
                if (validationErrors.Any())
                {
                    TempData["ErrorMessage"] = string.Join("; ", validationErrors);
                    CurrentTab = "create";
                    LoadBookClubData();
                    return Page();
                }

                NewBookClub.OwnerId = userId;
                int bookClubId = _bookClubBLL.CreateBookClub(NewBookClub);

                if (bookClubId > 0)
                {
                    TempData["SuccessMessage"] = $"Book club '{NewBookClub.Name}' created successfully!";
                    return RedirectToPage("/BookClub", new { id = bookClubId });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to create book club. Please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            CurrentTab = "create";
            LoadBookClubData();
            return Page();
        }

        public IActionResult OnPostJoinClub(int bookClubId)
        {
            var userId = GetCurrentUserId();
            if (userId <= 0)
            {
                return new JsonResult(new { success = false, message = "You must be logged in to join a book club." });
            }

            try
            {
                bool success = _bookClubBLL.JoinBookClub(bookClubId, userId);
                if (success)
                {
                    return new JsonResult(new { success = true, message = "Your request to join the book club has been submitted. Awaiting admin approval." });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Unable to join book club." });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public IActionResult OnPostLeaveClub(int bookClubId)
        {
            var userId = GetCurrentUserId();
            if (userId <= 0)
            {
                return new JsonResult(new { success = false, message = "You must be logged in." });
            }

            try
            {
                bool success = _bookClubBLL.LeaveBookClub(bookClubId, userId);
                if (success)
                {
                    return new JsonResult(new { success = true, message = "Successfully left the book club." });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Unable to leave book club." });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        private void LoadBookClubData()
        {
            var userId = GetCurrentUserId();
            
            try
            {
                // Load all public book clubs
                BookClubs = _bookClubBLL.GetAllBookClubs();
                
                if (userId > 0)
                {
                    // Load user's book clubs
                    LoadUserBookClubs();
                    
                    // Get membership status for each club
                    foreach (var club in BookClubs)
                    {
                        MembershipStatus[club.ClubId] = _bookClubBLL.GetMembershipStatus(club.ClubId, userId);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading book clubs: {ex.Message}";
                BookClubs = new List<BookClub>();
                UserBookClubs = new List<BookClub>();
            }
        }

        private void LoadUserBookClubs()
        {
            var userId = GetCurrentUserId();
            if (userId > 0)
            {
                try
                {
                    UserBookClubs = _bookClubBLL.GetUserBookClubs(userId);
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error loading your book clubs: {ex.Message}";
                    UserBookClubs = new List<BookClub>();
                }
            }
        }

        private int GetCurrentUserId()
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    return userId;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        // Admin Management Actions

        public IActionResult OnPostApproveMember(int clubId, int userId)
        {
            try
            {
                int adminUserId = GetCurrentUserId();
                if (adminUserId == 0)
                {
                    TempData["ErrorMessage"] = "You must be logged in to perform this action.";
                    return RedirectToPage();
                }

                bool success = _bookClubBLL.ApproveMember(clubId, userId, adminUserId);
                if (success)
                {
                    TempData["SuccessMessage"] = "Member approved successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to approve member.";
                }
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "You do not have permission to manage members in this club.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error approving member: {ex.Message}";
            }

            return RedirectToPage(new { tab = "manage", clubId = clubId });
        }

        public IActionResult OnPostRemoveMember(int clubId, int userId)
        {
            try
            {
                int adminUserId = GetCurrentUserId();
                if (adminUserId == 0)
                {
                    TempData["ErrorMessage"] = "You must be logged in to perform this action.";
                    return RedirectToPage();
                }

                bool success = _bookClubBLL.RemoveMember(clubId, userId, adminUserId);
                if (success)
                {
                    TempData["SuccessMessage"] = "Member removed successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to remove member.";
                }
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "You do not have permission to manage members in this club.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error removing member: {ex.Message}";
            }

            return RedirectToPage(new { tab = "manage", clubId = clubId });
        }

        public IActionResult OnPostChangeRole(int clubId, int userId, string newRole)
        {
            try
            {
                int ownerUserId = GetCurrentUserId();
                if (ownerUserId == 0)
                {
                    TempData["ErrorMessage"] = "You must be logged in to perform this action.";
                    return RedirectToPage();
                }

                bool success = _bookClubBLL.ChangeMemberRole(clubId, userId, newRole, ownerUserId);
                if (success)
                {
                    TempData["SuccessMessage"] = $"Member role changed to {newRole} successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to change member role.";
                }
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "Only club owners can change member roles.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error changing role: {ex.Message}";
            }

            return RedirectToPage(new { tab = "manage", clubId = clubId });
        }

        public IActionResult OnGetManageClub(int clubId)
        {
            try
            {
                int userId = GetCurrentUserId();
                if (userId == 0)
                {
                    TempData["ErrorMessage"] = "You must be logged in to manage clubs.";
                    return RedirectToPage();
                }

                // Check if user has admin privileges
                if (!_bookClubBLL.IsUserAdmin(clubId, userId))
                {
                    TempData["ErrorMessage"] = "You do not have permission to manage this club.";
                    return RedirectToPage();
                }

                // Load club management data
                LoadClubManagementData(clubId, userId);
                CurrentTab = "manage";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading club management: {ex.Message}";
            }

            return Page();
        }

        private void LoadClubManagementData(int clubId, int userId)
        {
            try
            {
                // Load club members and pending requests
                ClubMembers = _bookClubBLL.GetBookClubMembers(clubId);
                PendingMembers = _bookClubBLL.GetPendingMembers(clubId, userId);
                
                // Also load the user's clubs for the "My Clubs" tab
                LoadUserBookClubs();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading management data: {ex.Message}";
                ClubMembers = new List<BookClubMember>();
                PendingMembers = new List<BookClubMember>();
            }
        }

        private void LoadClubManagementData(int clubId)
        {
            if (string.IsNullOrEmpty(CurrentUserId) || !int.TryParse(CurrentUserId, out int userId)) return;

            SelectedClubId = clubId;
            
            try
            {
                // Get club details
                SelectedClub = _bookClubBLL.GetBookClubById(clubId);
                
                // Check if user is admin of this club
                if (!_bookClubBLL.IsUserAdmin(clubId, userId))
                {
                    // User is not admin, redirect or show error
                    return;
                }

                // Load pending members
                PendingMembers = _bookClubBLL.GetPendingMembers(clubId, userId);

                // Load current members
                CurrentMembers = _bookClubBLL.GetBookClubMembers(clubId)
                    .Where(m => m.IsApproved)
                    .ToList();
            }
            catch
            {
                // Handle error - could log here
                PendingMembers = new List<BookClubMember>();
                CurrentMembers = new List<BookClubMember>();
            }
        }
    }
}