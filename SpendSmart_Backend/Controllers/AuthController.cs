using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Services;

namespace SpendSmart_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, message, token) = await _authService.RegisterAsync(registerDto);

            var response = new AuthResponseDto
            {
                Success = success,
                Message = message,
                Token = token
            };

            if (!success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, message, token) = await _authService.LoginAsync(loginDto);

            var response = new AuthResponseDto
            {
                Success = success,
                Message = message,
                Token = token
            };

            if (!success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}