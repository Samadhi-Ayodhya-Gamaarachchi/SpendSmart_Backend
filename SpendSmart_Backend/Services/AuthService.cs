using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.Models;
using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.DTOs;


namespace SpendSmart_Backend.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RegisterUser(RegisterDto dto)
        {
            if (dto.Password != dto.ConfirmPassword)
                return false;

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return false;

            var user = new User
            {
                UserName = dto.UserName,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Currency = dto.Currency
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RegisterAdmin(RegisterDto dto)
        {
            if (dto.Password != dto.ConfirmPassword)
                return false;

            if (await _context.Admins.AnyAsync(a => a.Email == dto.Email))
                return false;

            var admin = new Admin
            {
                UserName = dto.UserName,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> LoginUser(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                throw new Exception("User not found");

            // Fix: Use 'Password' property instead of 'PasswordHash'
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.Password);

            if (!isPasswordValid)
                throw new Exception("Incorrect password");

            // Generate JWT token (simple version)
            var token = $"fake-jwt-token-for-{user.UserName}";
            return token;
        }


        public async Task<string> LoginAdmin(LoginDto dto)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (admin == null)
                throw new Exception("User not found");

            // Fix: Use 'Password' property instead of 'PasswordHash'
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, admin.Password);

            if (!isPasswordValid)
                throw new Exception("Incorrect password");
            // Generate JWT token (simple version)
            var token = $"fake-jwt-token-for-{admin.UserName}";
            return token;
        }



        public string GenerateJwtToken(string username, string role)
        {
            var jwtSettings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Jwt");

            var claims = new[]
            {
        new Claim(ClaimTypes.Name, username),
        new Claim(ClaimTypes.Role, role)
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }

}
