using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SpendSmart_Backend.Controllers
{
    [Authorize]
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        /// <summary>
        /// Gets the current authenticated user's ID from JWT token
        /// </summary>
        protected int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid or missing user ID in token");
            }
            
            return userId;
        }

        /// <summary>
        /// Gets the current authenticated user's username from JWT token
        /// </summary>
        protected string GetCurrentUsername()
        {
            var username = User.FindFirst("username")?.Value;
            
            if (string.IsNullOrEmpty(username))
            {
                throw new UnauthorizedAccessException("Invalid or missing username in token");
            }
            
            return username;
        }

        /// <summary>
        /// Gets the current authenticated user's role from JWT token
        /// </summary>
        protected string GetCurrentUserRole()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            
            if (string.IsNullOrEmpty(role))
            {
                throw new UnauthorizedAccessException("Invalid or missing role in token");
            }
            
            return role;
        }

        /// <summary>
        /// Validates that the provided userId matches the authenticated user's ID
        /// This prevents users from accessing other users' data via URL manipulation
        /// </summary>
        protected bool ValidateUserAccess(int requestedUserId)
        {
            return GetCurrentUserId() == requestedUserId;
        }
    }
}