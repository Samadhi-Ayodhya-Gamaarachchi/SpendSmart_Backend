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

        // PUT: api/AdminProfile/{id} - UPDATE admin profile (Your main CRUD operation!)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfile(int id, AdminProfileUpdateDto adminProfileUpdateDto)
        {
            // Find the admin by id
            var admin = await _context.Admins.FindAsync(id);
            
            if (admin == null)
            {
                return NotFound($"Admin with ID {id} not found.");
            }

            // Security Check 1: Verify current password if provided
            if (!string.IsNullOrWhiteSpace(adminProfileUpdateDto.CurrentPassword))
            {
                var hashedCurrentPassword = HashPassword(adminProfileUpdateDto.CurrentPassword);
                if (admin.Password != hashedCurrentPassword)
                {
                    return BadRequest(new { message = "Current password is incorrect." });
                }
            }
            else if (!string.IsNullOrWhiteSpace(adminProfileUpdateDto.Password))
            {
                // Require current password when changing password
                return BadRequest(new { message = "Current password is required to change password." });
            }

            // Security Check 2: Email uniqueness validation
            if (!string.IsNullOrWhiteSpace(adminProfileUpdateDto.Email) && 
                adminProfileUpdateDto.Email != admin.Email)
            {
                var existingAdmin = await _context.Admins
                    .FirstOrDefaultAsync(a => a.Email == adminProfileUpdateDto.Email && a.Id != id);
                
                if (existingAdmin != null)
                {
                    return BadRequest(new { message = "Email address is already in use by another admin." });
                }
            }

            // Map DTO properties to Admin entity
            admin.Name = adminProfileUpdateDto.Name;
            admin.Email = adminProfileUpdateDto.Email;
            
            // Only update password if it's provided
            if (!string.IsNullOrWhiteSpace(adminProfileUpdateDto.Password))
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
    }
}
