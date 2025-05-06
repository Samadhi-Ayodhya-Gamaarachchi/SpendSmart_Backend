using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Models;
using System.Security.Cryptography;

namespace SpendSmart_Backend.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, string Token)> RegisterAsync(RegisterDto registerDto);
        Task<(bool Success, string Message, string Token)> LoginAsync(LoginDto loginDto);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<(bool Success, string Message, string Token)> RegisterAsync(RegisterDto registerDto)
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return (false, "Email already in use.", null);
            }

            // Hash the password
            string passwordHash = HashPassword(registerDto.Password);

            // Create new user
            var user = new User
            {
                FirstName = registerDto.Name, // Using Name as FirstName for now
                LastName = "", // You might want to split Name or add a LastName field to RegisterDto
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.Now
            };

            // Add user to database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate JWT token
            string token = GenerateJwtToken(user);

            return (true, "Registration successful", token);
        }

        public async Task<(bool Success, string Message, string Token)> LoginAsync(LoginDto loginDto)
        {
            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            if (user == null)
            {
                return (false, "Invalid email or password.", null);
            }

            // Verify password
            if (!VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return (false, "Invalid email or password.", null);
            }

            // Generate JWT token
            string token = GenerateJwtToken(user);

            return (true, "Login successful", token);
        }

        private string HashPassword(string password)
        {
            // Simple password hashing using HMACSHA512
            // In a production environment, consider using a more specialized library like BCrypt
            using (var hmac = new HMACSHA512())
            {
                var salt = hmac.Key;
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Combine salt and hash for storage
                byte[] hashWithSalt = new byte[salt.Length + hash.Length];
                Buffer.BlockCopy(salt, 0, hashWithSalt, 0, salt.Length);
                Buffer.BlockCopy(hash, 0, hashWithSalt, salt.Length, hash.Length);

                return Convert.ToBase64String(hashWithSalt);
            }
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            // Decode stored hash+salt
            byte[] hashWithSalt = Convert.FromBase64String(storedHash);

            // Extract salt (first 64 bytes for HMACSHA512)
            byte[] salt = new byte[64];
            Buffer.BlockCopy(hashWithSalt, 0, salt, 0, salt.Length);

            // Extract original hash
            byte[] originalHash = new byte[hashWithSalt.Length - salt.Length];
            Buffer.BlockCopy(hashWithSalt, salt.Length, originalHash, 0, originalHash.Length);

            // Compute hash of provided password with the same salt
            using (var hmac = new HMACSHA512(salt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Compare computed hash with original hash
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != originalHash[i])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryInMinutes"])),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}