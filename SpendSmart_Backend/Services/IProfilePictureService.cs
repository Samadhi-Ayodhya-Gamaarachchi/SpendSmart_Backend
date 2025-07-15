using SpendSmart_Backend.DTOs;

namespace SpendSmart_Backend.Services
{
    public interface IProfilePictureService
    {
        Task<ProfilePictureResponseDto> UploadProfilePictureAsync(ProfilePictureUploadDto uploadDto);
        Task<DeleteProfilePictureResponseDto> DeleteProfilePictureAsync(int userId);
        Task<string?> GetProfilePictureUrlAsync(int userId);
        Task<UserProfileDto?> GetUserProfileAsync(int userId);
        Task<bool> UserExistsAsync(int userId);
    }
}
