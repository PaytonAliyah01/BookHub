using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BookHub.BLL;

namespace BookHub.Presentation.Pages
{
    [Authorize]
    public class EditProfileModel : PageModel
    {
        private readonly UserBLL _userBLL;
        private readonly IWebHostEnvironment _environment;

        public EditProfileModel(IConfiguration config, IWebHostEnvironment environment)
        {
            _userBLL = new UserBLL(config.GetConnectionString("DefaultConnection")!);
            _environment = environment;
        }

        [BindProperty]
        public string Name { get; set; } = "";

        [BindProperty]
        public string Bio { get; set; } = "";

        [BindProperty]
        public string CurrentProfileImage { get; set; } = "default.png";

        [BindProperty]
        public IFormFile? ProfileImageFile { get; set; }

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public IActionResult OnGet()
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email))
                {
                    return RedirectToPage("/Login");
                }

                // Load current user profile
                var userProfile = _userBLL.GetUserProfile(email);
                if (userProfile == null)
                {
                    ErrorMessage = "Unable to load user profile.";
                    return Page();
                }

                // Populate form with current data
                Name = userProfile.Name;
                Bio = userProfile.Bio;
                CurrentProfileImage = userProfile.ProfileImage;

                return Page();
            }
            catch (ApplicationException ex)
            {
                ErrorMessage = ex.Message;
                return Page();
            }
            catch (Exception)
            {
                ErrorMessage = "An unexpected error occurred while loading your profile.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email))
                {
                    return RedirectToPage("/Login");
                }

                // Presentation layer validation
                if (string.IsNullOrWhiteSpace(Name))
                {
                    ErrorMessage = "Name is required.";
                    return Page();
                }

                if (Name.Length < 2 || Name.Length > 100)
                {
                    ErrorMessage = "Name must be between 2 and 100 characters.";
                    return Page();
                }

                if (!string.IsNullOrEmpty(Bio) && Bio.Length > 500)
                {
                    ErrorMessage = "Bio cannot exceed 500 characters.";
                    return Page();
                }

                string profileImageFileName = CurrentProfileImage;

                // Handle profile image upload
                if (ProfileImageFile != null && ProfileImageFile.Length > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                    var extension = Path.GetExtension(ProfileImageFile.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(extension))
                    {
                        ErrorMessage = "Please upload a valid image file (jpg, jpeg, png, gif, bmp).";
                        return Page();
                    }

                    // Validate file size (5MB limit)
                    if (ProfileImageFile.Length > 5 * 1024 * 1024)
                    {
                        ErrorMessage = "Profile image must be smaller than 5MB.";
                        return Page();
                    }

                    // Get user ID for unique filename
                    var userProfile = _userBLL.GetUserProfile(email);
                    if (userProfile != null)
                    {
                        // Generate unique filename
                        profileImageFileName = _userBLL.GenerateProfileImageFileName(
                            ProfileImageFile.FileName, userProfile.UserId);

                        // Save file to wwwroot/images/profiles
                        var uploadsPath = Path.Combine(_environment.WebRootPath, "images", "profiles");
                        if (!Directory.Exists(uploadsPath))
                        {
                            Directory.CreateDirectory(uploadsPath);
                        }

                        var filePath = Path.Combine(uploadsPath, profileImageFileName);

                        // Delete old profile image if it's not default
                        if (CurrentProfileImage != "default.png")
                        {
                            var oldFilePath = Path.Combine(uploadsPath, CurrentProfileImage);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ProfileImageFile.CopyToAsync(stream);
                        }
                    }
                }

                // Delegate to business logic
                if (_userBLL.UpdateProfile(email, Name, Bio, profileImageFileName))
                {
                    SuccessMessage = "Profile updated successfully!";
                    CurrentProfileImage = profileImageFileName;
                }
                else
                {
                    ErrorMessage = "Unable to update profile. Please check your information and try again.";
                }

                return Page();
            }
            catch (ApplicationException ex)
            {
                ErrorMessage = ex.Message;
                return Page();
            }
            catch (Exception)
            {
                ErrorMessage = "An unexpected error occurred while updating your profile.";
                return Page();
            }
        }
    }
}