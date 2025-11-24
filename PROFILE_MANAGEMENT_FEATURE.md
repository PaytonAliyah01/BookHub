# Profile Management Feature Implementation

## User Story
**As a user, I want to edit my profile (name, bio, profile picture).**

## Features Implemented

### 1. View Profile (`/Profile`)
- **Access**: Authenticated users only
- **Features**:
  - Display current profile information (name, email, bio)
  - Show profile picture or default image
  - Quick access buttons to edit profile and change password
  - Clean, responsive layout with profile picture display

### 2. Edit Profile (`/EditProfile`)
- **Access**: Authenticated users only
- **Features**:
  - Update name (2-100 characters)
  - Update bio (up to 500 characters with live counter)
  - Upload and change profile picture
  - Image validation and file size limits (5MB)
  - Real-time character counting for bio field
  - Secure file upload with unique naming

### 3. Enhanced Navigation
- **User Dropdown Menu**:
  - View Profile option
  - Edit Profile option
  - Change Password option
  - Logout option
- **Bootstrap Icons**: Professional iconography throughout
- **Responsive Design**: Works on all screen sizes

## Technical Implementation

### Data Access Layer (DAL)

**Enhanced User Model:**
```csharp
public class User
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;           // NEW
    public string ProfileImage { get; set; } = "default.png";
}
```

**New Methods in UserDAL:**
```csharp
// Update user profile information
void UpdateProfile(string email, string name, string bio, string profileImage)

// Get user by ID for profile management
User? GetUserById(int userId)

// Enhanced GetUserWithCredentials to include bio
User? GetUserWithCredentials(string email) // Updated to include Bio
```

**Features:**
- Full exception handling for all profile operations
- Input validation at data access level
- Safe SQL parameter binding
- Atomic profile updates
- Enhanced user retrieval methods

### Business Logic Layer (BLL)

**New Methods in UserBLL:**
```csharp
// Update user profile with business validation
bool UpdateProfile(string email, string name, string bio, string profileImage)

// Get user profile by email
User? GetUserProfile(string email)

// Get user profile by ID
User? GetUserProfile(int userId)

// Validate image file extensions
bool IsValidImageFile(string fileName)

// Generate unique profile image filenames
string GenerateProfileImageFileName(string originalFileName, int userId)
```

**Business Rules Implemented:**
- Name length validation (2-100 characters)
- Bio length validation (max 500 characters)
- Image file type validation (jpg, jpeg, png, gif, bmp)
- File size validation (5MB limit)
- Unique file naming to prevent conflicts
- User existence verification before updates

### Presentation Layer

**New Pages:**

1. **Profile.cshtml**: Read-only profile view with professional layout
2. **EditProfile.cshtml**: Comprehensive profile editing form

**Features:**
- **File Upload Handling**: Secure multipart form processing
- **Image Management**: Automatic old image cleanup
- **Real-time Validation**: Character counting and client-side feedback
- **Responsive Design**: Bootstrap 5 styling with custom CSS
- **Professional UI**: Card-based layouts with icons and proper spacing

## Security Features

### 1. Authentication & Authorization
- All profile pages require active login session
- User identity verification through claims
- Session-based security for profile operations

### 2. File Upload Security
- **File Type Validation**: Only allows image file types
- **File Size Limits**: 5MB maximum to prevent abuse
- **Unique File Naming**: Prevents filename collisions and overwriting
- **Directory Security**: Files stored in protected wwwroot/images/profiles
- **Old File Cleanup**: Automatic deletion of replaced profile images

### 3. Input Validation
- **Multi-layer Validation**: Presentation → BLL → DAL
- **SQL Injection Prevention**: Parameterized queries
- **XSS Prevention**: Proper data encoding in views
- **Business Rule Enforcement**: Name/bio length limits

### 4. Data Protection
- **Secure File Handling**: Async file operations
- **Error Information Hiding**: Generic error messages for users
- **User Isolation**: Users can only edit their own profiles

## User Experience Flow

### View Profile Flow:
1. User logs in and navigates to dropdown → "View Profile"
2. System displays current profile information
3. Clean layout shows profile picture, name, email, and bio
4. Quick action buttons for editing profile or changing password

### Edit Profile Flow:
1. User navigates to "Edit Profile" from dropdown or profile view
2. Form pre-populated with current profile data
3. User can:
   - Update name with real-time validation
   - Update bio with character counter (500 max)
   - Upload new profile picture with preview
4. Client-side validation provides immediate feedback
5. Server-side processing handles file upload and data update
6. Success message confirms changes with updated display

## File Management

### Profile Image Handling:
- **Storage Location**: `wwwroot/images/profiles/`
- **Naming Convention**: `profile_{userId}_{timestamp}.{extension}`
- **Default Image**: Falls back to `default.png` if no image uploaded
- **Cleanup Process**: Automatically removes old profile images when updated
- **Security**: File type and size validation prevents malicious uploads

### Directory Structure:
```
wwwroot/
├── images/
│   ├── profiles/           # User profile images
│   │   ├── profile_1_20251113143022.jpg
│   │   └── profile_2_20251113143105.png
│   └── default-profile.png # Default profile image
```

## Database Schema Requirements

Update Users table to include Bio field:
```sql
ALTER TABLE Users 
ADD Bio NVARCHAR(500) DEFAULT '';

-- Full table structure
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) UNIQUE NOT NULL,
    Bio NVARCHAR(500) DEFAULT '',
    PasswordHash NVARCHAR(255) NOT NULL,
    Salt NVARCHAR(255) NOT NULL,
    ProfileImage NVARCHAR(255) DEFAULT 'default.png',
    CreatedDate DATETIME2 DEFAULT GETUTCDATE()
);
```

## Error Handling

### DAL Layer:
- SQL exceptions converted to `InvalidOperationException`
- Database connection failure handling
- Data integrity validation
- File system operation error handling

### BLL Layer:
- DAL exceptions converted to `ApplicationException`
- Business rule validation with meaningful messages
- File validation and processing errors

### Presentation Layer:
- Application exceptions displayed as user-friendly messages
- File upload error handling
- Form validation feedback
- Graceful degradation on errors

## Production Considerations

### Performance Optimizations:
1. **Image Compression**: Implement client-side image resizing
2. **CDN Integration**: Store profile images on CDN for better performance
3. **Caching**: Cache user profile data for faster loading
4. **Lazy Loading**: Load profile images on demand

### Security Enhancements:
1. **Image Scanning**: Virus/malware scanning for uploaded files
2. **Content Validation**: Deep file content validation beyond extension
3. **Rate Limiting**: Prevent profile update abuse
4. **Audit Logging**: Log all profile changes for security

### Storage Considerations:
1. **Cloud Storage**: Move to AWS S3/Azure Blob for scalability
2. **Backup Strategy**: Regular backup of profile images
3. **Storage Quotas**: Implement per-user storage limits

## Testing Scenarios

### Manual Testing:
1. **Profile View**: Login → View profile with all fields displayed correctly
2. **Profile Edit**: Update name, bio, and profile picture → Verify changes
3. **File Upload**: Upload various image types and sizes → Verify validation
4. **Form Validation**: Test character limits and required fields
5. **Navigation**: Test all profile-related links and buttons

### Error Scenarios:
1. File upload failures (wrong type, too large)
2. Database connection issues during updates
3. Invalid user sessions
4. Missing or corrupted profile images
5. Form submission with invalid data

### Security Testing:
1. Upload malicious files (should be rejected)
2. Attempt to edit other users' profiles (should fail)
3. Test SQL injection attempts in form fields
4. Verify file overwrite protection

This implementation provides a complete, secure, and user-friendly profile management system that follows modern web development best practices while maintaining the 3-layer architecture.