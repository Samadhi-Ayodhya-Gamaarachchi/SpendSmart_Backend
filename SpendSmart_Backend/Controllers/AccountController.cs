using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Services;

namespace SpendSmart_Backend.Controllers { 

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : Controller

    {
        private readonly IAccountService _accountService, _userService;
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;

        }

        [HttpGet]
        public async Task<ActionResult<AccountDto>> GetAccount()
        {
            // Get the current user's ID from the token claims
            int userId = GetCurrentUserId();

            var account = await _accountService.GetUserAccountAsync(userId);
            if (account == null)
            {
                return NotFound();
            }

            return account;

        }

        [HttpPut]
        public async Task<IActionResult> UpdateAccount(UpdateAccountDto updateAccountDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            int userId = GetCurrentUserId();

            try
            {
                var result = await _accountService.UpdateUserAccountAsync(userId, updateAccountDto);
                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return 0; // Or handle this case differently
            }

            return int.Parse(userIdClaim);
        }
    }
}
