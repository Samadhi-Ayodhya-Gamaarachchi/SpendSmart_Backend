using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.Models;
using SpendSmart_Backend.Models.DTOs;
using SpendSmart_Backend.Services;
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
        private readonly IEmailService _emailService;

        public AdminProfileController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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

        // PUT: api/AdminProfile/{id} - UPDATE admin profile with EMAIL VERIFICATION
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfile(int id, AdminProfileUpdateDto adminProfileUpdateDto)
        {
            // Debug logging
            Console.WriteLine($"UpdateProfile called with ID: {id}");
            Console.WriteLine($"Name: '{adminProfileUpdateDto.Name}'");
            Console.WriteLine($"Email: '{adminProfileUpdateDto.Email}'");
            Console.WriteLine($"Password provided: {!string.IsNullOrEmpty(adminProfileUpdateDto.Password)}");
            Console.WriteLine($"CurrentPassword provided: {!string.IsNullOrEmpty(adminProfileUpdateDto.CurrentPassword)}");

            // Find the admin by id
            var admin = await _context.Admins.FindAsync(id);
            
            if (admin == null)
            {
                return NotFound($"Admin with ID {id} not found.");
            }

            // Check if email is actually changing
            bool isEmailChanging = !string.IsNullOrWhiteSpace(adminProfileUpdateDto.Email) && 
                                   adminProfileUpdateDto.Email != admin.Email;

            if (isEmailChanging)
            {
                // Check email uniqueness
                var existingAdmin = await _context.Admins
                    .FirstOrDefaultAsync(a => a.Email == adminProfileUpdateDto.Email && a.Id != id);
                
                if (existingAdmin != null)
                {
                    return BadRequest(new { message = "Email address is already in use by another admin." });
                }

                // EMAIL VERIFICATION REQUIRED - Don't update email immediately
                try
                {
                    Console.WriteLine("Starting email verification process...");
                    
                    // Generate verification token
                    var verificationToken = GenerateVerificationToken();
                    Console.WriteLine($"Generated verification token: {verificationToken.Substring(0, 10)}...");
                    
                    // Remove any existing verification for this admin
                    var existingVerification = await _context.EmailVerifications
                        .FirstOrDefaultAsync(ev => ev.AdminId == id);
                    if (existingVerification != null)
                    {
                        Console.WriteLine("Removing existing verification...");
                        _context.EmailVerifications.Remove(existingVerification);
                    }
                    
                    // Create new email verification record
                    var emailVerification = new EmailVerification
                    {
                        AdminId = id,
                        NewEmail = adminProfileUpdateDto.Email ?? string.Empty,
                        VerificationToken = verificationToken,
                        CreatedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddHours(24)
                    };
                    
                    Console.WriteLine($"Created verification record for email: {emailVerification.NewEmail}");
                    _context.EmailVerifications.Add(emailVerification);
                    
                    // Update admin's pending email (but not the actual email yet)
                    admin.PendingEmail = adminProfileUpdateDto.Email;
                    Console.WriteLine($"Set pending email to: {admin.PendingEmail}");
                    
                    // Save changes before sending email
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Saved verification record to database");
                    
                    // Send verification email
                    Console.WriteLine("Attempting to send verification email...");
                    await _emailService.SendVerificationEmailAsync(emailVerification);
                    
                    Console.WriteLine($"Email verification sent successfully to: {adminProfileUpdateDto.Email}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send email verification: {ex.Message}");
                    return BadRequest(new { message = "Failed to send verification email. Please try again later." });
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
                
                // Update password
                if (!string.IsNullOrEmpty(adminProfileUpdateDto.Password))
                {
                    admin.Password = HashPassword(adminProfileUpdateDto.Password);
                }
            }

            // Always update name (no verification needed)
            if (!string.IsNullOrEmpty(adminProfileUpdateDto.Name))
            {
                admin.Name = adminProfileUpdateDto.Name;
            }
            
            _context.Entry(admin).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                
                // Return different responses based on whether email verification is required
                if (isEmailChanging)
                {
                    return Ok(new { 
                        message = "Profile updated. Email verification sent to new email address.",
                        emailVerificationRequired = true,
                        pendingEmail = adminProfileUpdateDto.Email
                    });
                }
                else
                {
                    return Ok(new { message = "Admin profile updated successfully!" });
                }
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

        // ==================== EMAIL VERIFICATION ENDPOINTS ====================

        // Helper method to generate verification token
        private string GenerateVerificationToken()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[32]; // 256 bits
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
            }
        }

        // GET: api/AdminProfile/{id}/email-verification-status
        [HttpGet("{id}/email-verification-status")]
        public async Task<ActionResult> GetEmailVerificationStatus(int id)
        {
            try
            {
                var admin = await _context.Admins.FindAsync(id);
                if (admin == null)
                {
                    return NotFound($"Admin with ID {id} not found.");
                }

                var pendingVerification = await _context.EmailVerifications
                    .FirstOrDefaultAsync(ev => ev.AdminId == id && !ev.IsVerified && ev.ExpiresAt > DateTime.UtcNow);

                return Ok(new
                {
                    hasPendingVerification = pendingVerification != null,
                    pendingEmail = pendingVerification?.NewEmail ?? admin.PendingEmail
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking verification status: {ex.Message}");
                return BadRequest(new { error = "Failed to check verification status", details = ex.Message });
            }
        }

        // POST: api/AdminProfile/{id}/resend-verification
        [HttpPost("{id}/resend-verification")]
        public async Task<ActionResult> ResendEmailVerification(int id)
        {
            try
            {
                var admin = await _context.Admins.FindAsync(id);
                if (admin == null)
                {
                    return NotFound($"Admin with ID {id} not found.");
                }

                var pendingVerification = await _context.EmailVerifications
                    .FirstOrDefaultAsync(ev => ev.AdminId == id && !ev.IsVerified);

                if (pendingVerification == null)
                {
                    return BadRequest(new { message = "No pending email verification found." });
                }

                // Check rate limiting (don't allow resend more than once every 60 seconds)
                if ((DateTime.UtcNow - pendingVerification.CreatedAt).TotalSeconds < 60)
                {
                    return BadRequest(new { message = "Please wait before requesting another verification email." });
                }

                // Generate new token and update expiry
                pendingVerification.VerificationToken = GenerateVerificationToken();
                pendingVerification.CreatedAt = DateTime.UtcNow;
                pendingVerification.ExpiresAt = DateTime.UtcNow.AddHours(24);

                var verificationUrl = $"http://localhost:3000/verify-email?token={pendingVerification.VerificationToken}&adminId={id}";
                await _emailService.SendVerificationEmailAsync(pendingVerification);

                await _context.SaveChangesAsync();

                return Ok(new { message = "Verification email resent successfully!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resending verification: {ex.Message}");
                return BadRequest(new { error = "Failed to resend verification email", details = ex.Message });
            }
        }

        // POST: api/AdminProfile/{id}/cancel-verification
        [HttpPost("{id}/cancel-verification")]
        public async Task<ActionResult> CancelEmailVerification(int id)
        {
            try
            {
                var admin = await _context.Admins.FindAsync(id);
                if (admin == null)
                {
                    return NotFound($"Admin with ID {id} not found.");
                }

                var pendingVerification = await _context.EmailVerifications
                    .FirstOrDefaultAsync(ev => ev.AdminId == id && !ev.IsVerified);

                if (pendingVerification != null)
                {
                    _context.EmailVerifications.Remove(pendingVerification);
                }

                // Clear pending email
                admin.PendingEmail = null;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Email verification cancelled successfully!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cancelling verification: {ex.Message}");
                return BadRequest(new { error = "Failed to cancel verification", details = ex.Message });
            }
        }

        // GET: api/AdminProfile/verify-email/{token}
        [HttpGet("verify-email/{token}")]
        public async Task<ActionResult> VerifyEmail(string token)
        {
            try
            {
                Console.WriteLine($"VerifyEmail called with token: {token.Substring(0, 10)}...");
                
                var verification = await _context.EmailVerifications
                    .Include(ev => ev.Admin)
                    .FirstOrDefaultAsync(ev => ev.VerificationToken == token);

                if (verification == null)
                {
                    Console.WriteLine("Verification token not found in database");
                    return BadRequest(new { message = "Invalid verification token." });
                }

                Console.WriteLine($"Found verification for admin ID: {verification.AdminId}, email: {verification.NewEmail}");

                if (verification.ExpiresAt < DateTime.UtcNow)
                {
                    Console.WriteLine($"Verification token expired. Expires: {verification.ExpiresAt}, Now: {DateTime.UtcNow}");
                    return BadRequest(new { message = "Verification token has expired." });
                }

                if (verification.IsVerified)
                {
                    Console.WriteLine("Verification token already used");
                    return BadRequest(new { message = "Email has already been verified." });
                }

                Console.WriteLine("Verification checks passed, proceeding with email update...");

                // Check if the new email is still available
                var existingAdmin = await _context.Admins
                    .FirstOrDefaultAsync(a => a.Email == verification.NewEmail && a.Id != verification.AdminId);
                
                if (existingAdmin != null)
                {
                    Console.WriteLine($"Email {verification.NewEmail} is already in use by admin ID: {existingAdmin.Id}");
                    return BadRequest(new { message = "Email address is no longer available." });
                }

                Console.WriteLine("Email availability check passed, updating admin email...");

                // Update admin email and mark verification as complete
                verification.Admin.Email = verification.NewEmail;
                verification.Admin.PendingEmail = null;
                verification.IsVerified = true;
                verification.VerifiedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                Console.WriteLine($"Email verification completed successfully! Updated email to: {verification.NewEmail}");

                return Ok(new { message = "Email verified successfully!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verifying email: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest(new { error = "Failed to verify email", details = ex.Message });
            }
        }
    }
}
