using Microsoft.AspNetCore.Mvc;
using SpendSmart_Backend.Services;
using SpendSmart_Backend.DTOs;
using System.ComponentModel.DataAnnotations;

namespace SpendSmart_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProfilePictureController : ControllerBase
    {
        private readonly IProfilePictureService _profilePictureService;
        private readonly ILogger<ProfilePictureController> _logger;

        public ProfilePictureController(
            IProfilePictureService profilePictureService,
            ILogger<ProfilePictureController> logger)
        {
            _profilePictureService = profilePictureService;
            _logger = logger;
        }

        /// <summary>
        /// Upload a profile picture for a user
        /// </summary>
        /// <param name="file">Image file to upload (JPEG, PNG, GIF, WebP, max 5MB)</param>
        /// <param name="userId">User ID</param>
        /// <returns>Upload result with URL if successful</returns>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ProfilePictureResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProfilePictureResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadProfilePicture(
    IFormFile file,      
    int userId)          
        {
            try
            {
                _logger.LogInformation($"Upload request received for user {userId}");

                if (userId <= 0)
                {
                    return BadRequest(new ProfilePictureResponseDto
                    {
                        Success = false,
                        Message = "Invalid user ID"
                    });
                }

                // ADD file validation here
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new ProfilePictureResponseDto
                    {
                        Success = false,
                        Message = "No file provided"
                    });
                }

                var uploadDto = new ProfilePictureUploadDto
                {
                    File = file,
                    UserId = userId
                };

                var result = await _profilePictureService.UploadProfilePictureAsync(uploadDto);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UploadProfilePicture endpoint");
                return StatusCode(500, new ProfilePictureResponseDto
                {
                    Success = false,
                    Message = "Internal server error occurred"
                });
            }
        }

        /// <summary>
        /// Update profile picture URL (for Firebase integration)
        /// </summary>
        /// <param name="request">Update profile picture URL request</param>
        /// <returns>Update result</returns>
        [HttpPut("update-url")]
        [ProducesResponseType(typeof(ProfilePictureResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProfilePictureResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProfilePictureUrl([FromBody] UpdateProfilePictureUrlDto request)
        {
            try
            {
                _logger.LogInformation($"Update profile picture URL request received for user {request.UserId}");

                if (request.UserId <= 0)
                {
                    return BadRequest(new ProfilePictureResponseDto
                    {
                        Success = false,
                        Message = "Invalid user ID"
                    });
                }

                var result = await _profilePictureService.UpdateProfilePictureUrlAsync(request);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateProfilePictureUrl endpoint");
                return StatusCode(500, new ProfilePictureResponseDto
                {
                    Success = false,
                    Message = "Internal server error occurred"
                });
            }
        }


        /// <summary>
        /// Delete a user's profile picture
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("delete/{userId}")]
        [ProducesResponseType(typeof(DeleteProfilePictureResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DeleteProfilePictureResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DeleteProfilePictureResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteProfilePicture([Required] int userId)
        {
            try
            {
                _logger.LogInformation($"Delete request received for user {userId}");

                if (userId <= 0)
                {
                    return BadRequest(new DeleteProfilePictureResponseDto
                    {
                        Success = false,
                        Message = "Invalid user ID"
                    });
                }

                var userExists = await _profilePictureService.UserExistsAsync(userId);
                if (!userExists)
                {
                    return NotFound(new DeleteProfilePictureResponseDto
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                var result = await _profilePictureService.DeleteProfilePictureAsync(userId);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteProfilePicture endpoint");
                return StatusCode(500, new DeleteProfilePictureResponseDto
                {
                    Success = false,
                    Message = "Internal server error occurred"
                });
            }
        }

        /// <summary>
        /// Get the profile picture URL for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Profile picture URL</returns>
        [HttpGet("url/{userId}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProfilePictureUrl([Required] int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new { message = "Invalid user ID" });
                }

                var userExists = await _profilePictureService.UserExistsAsync(userId);
                if (!userExists)
                {
                    return NotFound(new { message = "User not found" });
                }

                var profilePictureUrl = await _profilePictureService.GetProfilePictureUrlAsync(userId);

                return Ok(new
                {
                    userId = userId,
                    profilePictureUrl = profilePictureUrl,
                    hasProfilePicture = !string.IsNullOrEmpty(profilePictureUrl)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetProfilePictureUrl endpoint");
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        /// <summary>
        /// Get complete user profile including profile picture
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User profile information</returns>
        [HttpGet("profile/{userId}")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserProfile([Required] int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new { message = "Invalid user ID" });
                }

                var userProfile = await _profilePictureService.GetUserProfileAsync(userId);

                if (userProfile == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserProfile endpoint");
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }

        /// <summary>
        /// Check if a user exists
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User existence status</returns>
        [HttpGet("user-exists/{userId}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckUserExists([Required] int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new { message = "Invalid user ID" });
                }

                var exists = await _profilePictureService.UserExistsAsync(userId);

                return Ok(new
                {
                    userId = userId,
                    exists = exists
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckUserExists endpoint");
                return StatusCode(500, new { message = "Internal server error occurred" });
            }
        }
    }
}
