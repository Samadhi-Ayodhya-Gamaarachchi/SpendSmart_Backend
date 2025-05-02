using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Services;

namespace SpendSmart_Backend.Controllers
{
    [ApiController]
    [Route("api/user/auth")]
    public class UserAuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public UserAuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var success = await _authService.RegisterUser(dto);
            if (!success) return BadRequest("Registration failed");
            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var valid = await _authService.ValidateUser(dto);
            if (!valid) return Unauthorized("Invalid credentials");

            var token = _authService.GenerateJwtToken(dto.Email, "User");
            return Ok(new { token });
        }



    }

}
