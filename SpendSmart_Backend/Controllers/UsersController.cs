using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.Models;

namespace SpendSmart_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ApplicationDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Users
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            _logger.LogInformation("Getting all users");

            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.Currency
                })
                .ToListAsync();

            _logger.LogInformation("Returning {Count} users", users.Count);
            return Ok(users);
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<object>> GetUser(int id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.Currency
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", id);
                return NotFound();
            }

            return Ok(user);
        }

        // GET: api/Users/5/exists
        [HttpGet("{id}/exists")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> CheckUserExists(int id)
        {
            var exists = await _context.Users.AnyAsync(u => u.Id == id);
            return Ok(new { UserId = id, Exists = exists });
        }

        // POST: api/Users
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            _logger.LogInformation("Creating new user: {UserName}", createUserDto.UserName);

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state: {@Errors}", ModelState);
                    return BadRequest(ModelState);
                }

                // Check if username or email already exists
                var existingUser = await _context.Users
                    .AnyAsync(u => u.UserName == createUserDto.UserName || u.Email == createUserDto.Email);

                if (existingUser)
                {
                    _logger.LogWarning("User with username {UserName} or email {Email} already exists", 
                        createUserDto.UserName, createUserDto.Email);
                    return BadRequest("Username or email already exists.");
                }

                var user = new User
                {
                    UserName = createUserDto.UserName,
                    Password = createUserDto.Password, // In a real app, this should be hashed
                    FirstName = createUserDto.FirstName,
                    LastName = createUserDto.LastName,
                    Email = createUserDto.Email,
                    Currency = createUserDto.Currency ?? "USD"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User created successfully with ID: {UserId}", user.Id);

                var userResponse = new
                {
                    user.Id,
                    user.UserName,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.Currency
                };

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, "An error occurred while creating the user");
            }
        }
    }

    public class CreateUserDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; }
        public string? Currency { get; set; }
    }
}