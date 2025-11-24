using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.BLL;
using BookHub.DAL;

namespace BookHub.Presentation.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly ReadingGoalBLL _readingGoalBLL;
    private readonly UserDAL _userDAL;

    public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
    {
        _logger = logger;
        string connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        _readingGoalBLL = new ReadingGoalBLL(connectionString);
        _userDAL = new UserDAL(connectionString);
    }

    public BookHub.DAL.ReadingGoal? CurrentGoal { get; set; }

    public void OnGet()
    {
        var currentUser = GetCurrentUser();
        if (currentUser != null)
        {
            CurrentGoal = _readingGoalBLL.GetCurrentYearGoal(currentUser.UserId);
        }
    }

    private User? GetCurrentUser()
    {
        try
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return _userDAL.GetUserById(userId);
            }
            return null;
        }
        catch
        {
            return null;
        }
    }
}
