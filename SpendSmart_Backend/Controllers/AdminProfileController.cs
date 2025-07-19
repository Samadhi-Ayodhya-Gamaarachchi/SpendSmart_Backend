using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.Models;
using SpendSmart_Backend.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

namespace SpendSmart_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/AdminProfile
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Admin>>> GetAllAdmins()
        {
            return await _context.Admins.ToListAsync();
        }

        // GET: api/AdminProfile/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Admin>> GetAdmin(int id)
        {
            var admin = await _context.Admins.FindAsync(id);

            if (admin == null)
            {
                return NotFound($"Admin with ID {id} not found.");
            }

            return admin;
        }

        // POST: api/AdminProfile - CREATE new admin
        [HttpPost]
        public async Task<ActionResult<Admin>> CreateAdmin(AdminCreateDto adminDto)
        {
            // Check if an admin with the same email already exists
            if (await _context.Admins.AnyAsync(a => a.Email == adminDto.Email))
            {
                return BadRequest("An admin with this email already exists.");
            }

            var admin = new Admin
            {
                Name = adminDto.Name,
                Email = adminDto.Email,
                Password = HashPassword(adminDto.Password)
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAdmin), new { id = admin.Id }, admin);
        }

        // PUT: api/AdminProfile/{id} - UPDATE admin profile (Optimized for performance!)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfile(int id, AdminProfileUpdateDto adminProfileUpdateDto)
        {
            // Find the admin by id
            var admin = await _context.Admins.FindAsync(id);
            
            if (admin == null)
            {
                return NotFound($"Admin with ID {id} not found.");
            }

            // PERFORMANCE OPTIMIZATION: Only validate email uniqueness if email is actually changing
            bool isEmailChanging = !string.IsNullOrWhiteSpace(adminProfileUpdateDto.Email) && 
                                   adminProfileUpdateDto.Email != admin.Email;

            if (isEmailChanging)
            {
                // Only check email uniqueness if email is actually changing
                var existingAdmin = await _context.Admins
                    .FirstOrDefaultAsync(a => a.Email == adminProfileUpdateDto.Email && a.Id != id);
                
                if (existingAdmin != null)
                {
                    return BadRequest(new { message = "Email address is already in use by another admin." });
                }
            }

            // Security Check: Verify current password if provided (only for password changes)
            bool isPasswordChanging = !string.IsNullOrWhiteSpace(adminProfileUpdateDto.Password);
            
            if (isPasswordChanging)
            {
                if (string.IsNullOrWhiteSpace(adminProfileUpdateDto.CurrentPassword))
                {
                    return BadRequest(new { message = "Current password is required to change password." });
                }
                
                var hashedCurrentPassword = HashPassword(adminProfileUpdateDto.CurrentPassword);
                if (admin.Password != hashedCurrentPassword)
                {
                    return BadRequest(new { message = "Current password is incorrect." });
                }
            }

            // Map DTO properties to Admin entity (only update changed fields)
            admin.Name = adminProfileUpdateDto.Name;
            if (isEmailChanging)
            {
                admin.Email = adminProfileUpdateDto.Email;
            }
            
            // Only update password if it's provided
            if (isPasswordChanging)
            {
                // Hash the password before storing
                admin.Password = HashPassword(adminProfileUpdateDto.Password);
            }

            _context.Entry(admin).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent(); // 204 No Content is the standard response for successful PUT operations
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AdminExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE: api/AdminProfile/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null)
            {
                return NotFound($"Admin with ID {id} not found.");
            }

            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AdminExists(int id)
        {
            return _context.Admins.Any(e => e.Id == id);
        }

        // Password hashing using SHA256
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // Convert the input string to a byte array and compute the hash
                byte[] hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                // Convert the byte array to a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    builder.Append(hashBytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // ==================== PROFILE PICTURE CRUD OPERATIONS ====================

        // GET: api/AdminProfile/{id}/profile-picture - Get admin profile picture
        [HttpGet("{id}/profile-picture")]
        public async Task<ActionResult<ProfilePictureResponseDto>> GetProfilePicture(int id)
        {
            try
            {
                var admin = await _context.Admins.FindAsync(id);
                if (admin == null)
                {
                    return NotFound($"Admin with ID {id} not found.");
                }

                if (string.IsNullOrEmpty(admin.ProfilePicture))
                {
                    return NotFound("No profile picture found for this admin.");
                }

                var response = new ProfilePictureResponseDto
                {
                    Base64Image = admin.ProfilePicture,
                    FileName = admin.ProfilePictureFileName,
                    UploadedAt = admin.ProfilePictureUploadedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to retrieve profile picture", details = ex.Message });
            }
        }

        // POST: api/AdminProfile/{id}/profile-picture - Upload new profile picture
        [HttpPost("{id}/profile-picture")]
        public async Task<ActionResult> UploadProfilePicture(int id, [FromForm] IFormFile file)
        {
            try
            {
                // Validate admin exists
                var admin = await _context.Admins.FindAsync(id);
                if (admin == null)
                {
                    return NotFound($"Admin with ID {id} not found.");
                }

                // Validate file
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file provided.");
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return BadRequest("Invalid file type. Only JPEG, PNG, and WebP images are allowed.");
                }

                // Validate file size (2MB max instead of 5MB for better performance)
                const int maxSize = 2 * 1024 * 1024; // 2MB
                if (file.Length > maxSize)
                {
                    return BadRequest("File size too large. Maximum size is 2MB.");
                }

                // Convert file to base64 with size optimization
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();
                
                // For better performance, you might want to resize large images here
                // For now, we'll just convert to base64
                var base64String = Convert.ToBase64String(fileBytes);
                var dataUrl = $"data:{file.ContentType};base64,{base64String}";

                // Update admin profile picture
                admin.ProfilePicture = dataUrl;
                admin.ProfilePictureFileName = file.FileName;
                admin.ProfilePictureUploadedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "Profile picture uploaded successfully", 
                    fileName = file.FileName,
                    uploadedAt = admin.ProfilePictureUploadedAt
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to upload profile picture", details = ex.Message });
            }
        }

        // PUT: api/AdminProfile/{id}/profile-picture - Update existing profile picture
        [HttpPut("{id}/profile-picture")]
        public async Task<ActionResult> UpdateProfilePicture(int id, [FromForm] IFormFile file)
        {
            // Same logic as upload - just different HTTP method for REST compliance
            return await UploadProfilePicture(id, file);
        }

        // DELETE: api/AdminProfile/{id}/profile-picture - Delete profile picture
        [HttpDelete("{id}/profile-picture")]
        public async Task<ActionResult> DeleteProfilePicture(int id)
        {
            try
            {
                var admin = await _context.Admins.FindAsync(id);
                if (admin == null)
                {
                    return NotFound($"Admin with ID {id} not found.");
                }

                if (string.IsNullOrEmpty(admin.ProfilePicture))
                {
                    return NotFound("No profile picture found to delete.");
                }

                // Clear profile picture data
                admin.ProfilePicture = null;
                admin.ProfilePictureFileName = null;
                admin.ProfilePictureUploadedAt = null;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Profile picture deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to delete profile picture", details = ex.Message });
            }
        }

        // GET: api/AdminProfile/{id}/profile-info - Get admin info including profile picture status
        [HttpGet("{id}/profile-info")]
        public async Task<ActionResult<AdminResponseDto>> GetAdminWithProfileInfo(int id)
        {
            try
            {
                var admin = await _context.Admins.FindAsync(id);
                if (admin == null)
                {
                    return NotFound($"Admin with ID {id} not found.");
                }

                var response = new AdminResponseDto
                {
                    Id = admin.Id,
                    Name = admin.Name,
                    Email = admin.Email,
                    ProfilePictureUrl = admin.ProfilePicture,
                    ProfilePictureFileName = admin.ProfilePictureFileName,
                    ProfilePictureUploadedAt = admin.ProfilePictureUploadedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to retrieve admin profile info", details = ex.Message });
            }
        }
    }
}
