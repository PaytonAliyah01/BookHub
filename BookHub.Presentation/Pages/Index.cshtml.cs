using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.BLL;
namespace BookHub.Presentation.Pages;
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IReadingGoalBLL _readingGoalBLL;
    private readonly IUserBLL _userBLL;
    public IndexModel(ILogger<IndexModel> logger, IReadingGoalBLL readingGoalBLL, IUserBLL userBLL)
    {
        _logger = logger;
        _readingGoalBLL = readingGoalBLL;
        _userBLL = userBLL;
    }
    public ReadingGoalDto? CurrentGoal { get; set; }
    public void OnGet()
    {
        var currentUser = GetCurrentUser();
        if (currentUser != null)
        {
            CurrentGoal = _readingGoalBLL.GetCurrentYearGoal(currentUser.UserId);
        }
    }
    private UserDto? GetCurrentUser()
    {
        try
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return _userBLL.GetUserById(userId);
            }
            return null;
        }
        catch
        {
            return null;
        }
    }
}
