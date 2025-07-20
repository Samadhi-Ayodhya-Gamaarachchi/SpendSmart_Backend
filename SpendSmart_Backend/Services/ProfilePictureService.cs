using SpendSmart_Backend.Data;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace SpendSmart_Backend.Services
{
    public class ProfilePictureService : IProfilePictureService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProfilePictureService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _uploadsPath;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

        public ProfilePictureService(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            ILogger<ProfilePictureService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
            _configuration = configuration;

            // Create uploads directory path (for fallback if needed)
            _uploadsPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "profile-pictures");

            // Ensure uploads directory exists
            EnsureUploadsDirectoryExists();
        }

        public async Task<ProfilePictureResponseDto> UploadProfilePictureAsync(ProfilePictureUploadDto uploadDto)
        {
            try
            {
                _logger.LogInformation($"Starting profile picture upload for user {uploadDto.UserId}");

                // Validate file
                var validationResult = ValidateFile(uploadDto.File);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning($"File validation failed for user {uploadDto.UserId}: {validationResult.ErrorMessage}");
                    return new ProfilePictureResponseDto
                    {
                        Success = false,
                        Message = validationResult.ErrorMessage
                    };
                }

                // Check if user exists
                var user = await _context.Users.FindAsync(uploadDto.UserId);
                if (user == null)
                {
                    _logger.LogWarning($"User {uploadDto.UserId} not found");
                    return new ProfilePictureResponseDto
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                // Generate unique filename
                var fileName = GenerateUniqueFileName(uploadDto.UserId, uploadDto.File.FileName);
                var filePath = Path.Combine(_uploadsPath, fileName);
                var relativePath = Path.Combine("uploads", "profile-pictures", fileName).Replace("\\", "/");

                // Save file to disk (as backup)
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadDto.File.CopyToAsync(stream);
                }

                // Generate URL (this will be replaced by Firebase URL from frontend)
                var baseUrl = GetBaseUrl();
                var profilePictureUrl = $"{baseUrl}/{relativePath}";

                // Update user in database
                user.ProfilePictureUrl = profilePictureUrl;
                user.ProfilePicturePath = relativePath;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Profile picture uploaded successfully for user {uploadDto.UserId}. File: {fileName}");

                return new ProfilePictureResponseDto
                {
                    Success = true,
                    Message = "Profile picture uploaded successfully!",
                    ProfilePictureUrl = profilePictureUrl,
                    FileName = fileName,
                    UploadedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading profile picture for user {uploadDto.UserId}");
                return new ProfilePictureResponseDto
                {
                    Success = false,
                    Message = $"An error occurred while uploading the profile picture: {ex.Message}"
                };
            }
        }

        public async Task<ProfilePictureResponseDto> UpdateProfilePictureUrlAsync(UpdateProfilePictureUrlDto updateDto)
        {
            try
            {
                _logger.LogInformation($"Updating profile picture URL for user {updateDto.UserId}");

                // Check if user exists
                var user = await _context.Users.FindAsync(updateDto.UserId);
                if (user == null)
                {
                    _logger.LogWarning($"User {updateDto.UserId} not found");
                    return new ProfilePictureResponseDto
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                // Update user profile picture URL (from Firebase)
                user.ProfilePictureUrl = updateDto.ProfilePictureUrl;
                user.ProfilePicturePath = updateDto.FileName; // Store Firebase filename
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Profile picture URL updated successfully for user {updateDto.UserId}");

                return new ProfilePictureResponseDto
                {
                    Success = true,
                    Message = updateDto.ProfilePictureUrl != null ?
                        "Profile picture URL updated successfully!" :
                        "Profile picture removed successfully!",
                    ProfilePictureUrl = updateDto.ProfilePictureUrl,
                    FileName = updateDto.FileName,
                    UploadedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating profile picture URL for user {updateDto.UserId}");
                return new ProfilePictureResponseDto
                {
                    Success = false,
                    Message = $"An error occurred while updating the profile picture URL: {ex.Message}"
                };
            }
        }

        public async Task<DeleteProfilePictureResponseDto> DeleteProfilePictureAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"Starting profile picture deletion for user {userId}");

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"User {userId} not found");
                    return new DeleteProfilePictureResponseDto
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                if (string.IsNullOrEmpty(user.ProfilePictureUrl))
                {
                    _logger.LogWarning($"No profile picture found for user {userId}");
                    return new DeleteProfilePictureResponseDto
                    {
                        Success = false,
                        Message = "No profile picture to delete"
                    };
                }

                // Delete file from disk if it exists (backup file)
                if (!string.IsNullOrEmpty(user.ProfilePicturePath))
                {
                    DeleteExistingProfilePictureFile(user.ProfilePicturePath);
                }

                // Update user in database
                user.ProfilePictureUrl = null;
                user.ProfilePicturePath = null;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Profile picture deleted successfully for user {userId}");

                return new DeleteProfilePictureResponseDto
                {
                    Success = true,
                    Message = "Profile picture deleted successfully!",
                    DeletedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting profile picture for user {userId}");
                return new DeleteProfilePictureResponseDto
                {
                    Success = false,
                    Message = $"An error occurred while deleting the profile picture: {ex.Message}"
                };
            }
        }

        public async Task<string?> GetProfilePictureUrlAsync(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.ProfilePictureUrl)
                    .FirstOrDefaultAsync();

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting profile picture URL for user {userId}");
                return null;
            }
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new UserProfileDto
                    {
                        Id = u.Id,
                        Email = u.Email,
                        ProfilePictureUrl = u.ProfilePictureUrl,
                        CreatedAt = u.CreatedAt,
                        UpdatedAt = u.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user profile for user {userId}");
                return null;
            }
        }

        public async Task<bool> UserExistsAsync(int userId)
        {
            try
            {
                return await _context.Users.AnyAsync(u => u.Id == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if user {userId} exists");
                return false;
            }
        }

        #region Private Methods

        private (bool IsValid, string ErrorMessage) ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return (false, "No file selected");
            }

            if (file.Length > _maxFileSize)
            {
                return (false, $"File size exceeds maximum limit of {_maxFileSize / (1024 * 1024)}MB");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                return (false, $"Invalid file type. Only {string.Join(", ", _allowedExtensions)} files are allowed");
            }

            // Additional content type validation
            var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return (false, "Invalid content type");
            }

            return (true, string.Empty);
        }

        private string GenerateUniqueFileName(int userId, string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var uniqueId = Guid.NewGuid().ToString("N")[..8];
            return $"user_{userId}_{timestamp}_{uniqueId}{extension}";
        }

        private void DeleteExistingProfilePictureFile(string relativePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, relativePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation($"Deleted existing profile picture file: {relativePath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Could not delete existing profile picture file: {relativePath}");
            }
        }

        private string GetBaseUrl()
        {
            return _configuration["BaseUrl"] ?? "https://localhost:7211";
        }

        private void EnsureUploadsDirectoryExists()
        {
            if (!Directory.Exists(_uploadsPath))
            {
                Directory.CreateDirectory(_uploadsPath);
                _logger.LogInformation($"Created uploads directory: {_uploadsPath}");
            }
        }

        #endregion
    }
}