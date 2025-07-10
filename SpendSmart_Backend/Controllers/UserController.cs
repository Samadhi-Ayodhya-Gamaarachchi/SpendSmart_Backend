using Microsoft.AspNetCore.Mvc;
using SpendSmart_Backend.Models;
using SpendSmart_Backend.DTOs;
using System.Linq;
using SpendSmart_Backend.Data;
using Microsoft.AspNetCore.Cors;

namespace SpendSmart_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("AllowReactApp")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPut("update-name")]
        public IActionResult UpdateName([FromBody] UpdateUserNameDto dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == dto.UserId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            user.UserName = dto.UserName;
            _context.SaveChanges();

            return Ok(new { message = "Name updated successfully" });
        }
        [HttpPut("update-email")]
        public IActionResult UpdateEmail([FromBody] UpdateUserEmailDto dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == dto.UserId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Optional: check if email already exists
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == dto.Email);
            if (existingUser != null && existingUser.Id != dto.UserId)
            {
                return Conflict("Email is already in use.");
            }

            user.Email = dto.Email;
            _context.SaveChanges();

            return Ok(new { message = "Email updated successfully" });
        }

    }
}