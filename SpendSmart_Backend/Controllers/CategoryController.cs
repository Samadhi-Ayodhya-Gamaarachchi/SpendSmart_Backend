// Controllers/CategoryController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.DTOs.Common;

namespace SpendSmart_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ApplicationDbContext context, ILogger<CategoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Category
        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<List<CategoryResponseDto>>>> GetAllCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .Select(c => new CategoryResponseDto
                    {
                        Id = c.Id,
                        CategoryName = c.CategoryName,
                        Type = c.Type
                    })
                    .OrderBy(c => c.CategoryName)
                    .ToListAsync();

                return Ok(ApiResponseDto<List<CategoryResponseDto>>.SuccessResponse(categories));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return StatusCode(500, ApiResponseDto<List<CategoryResponseDto>>.ErrorResponse("An error occurred while retrieving categories"));
            }
        }

        // GET: api/Category/type/{type}
        [HttpGet("type/{type}")]
        public async Task<ActionResult<ApiResponseDto<List<CategoryResponseDto>>>> GetCategoriesByType(string type)
        {
            try
            {
                var categories = await _context.Categories
                    .Where(c => c.Type.ToLower() == type.ToLower())
                    .Select(c => new CategoryResponseDto
                    {
                        Id = c.Id,
                        CategoryName = c.CategoryName,
                        Type = c.Type
                    })
                    .OrderBy(c => c.CategoryName)
                    .ToListAsync();

                return Ok(ApiResponseDto<List<CategoryResponseDto>>.SuccessResponse(categories));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories by type {Type}", type);
                return StatusCode(500, ApiResponseDto<List<CategoryResponseDto>>.ErrorResponse("An error occurred while retrieving categories"));
            }
        }

        // GET: api/Category/{categoryId}
        [HttpGet("{categoryId}")]
        public async Task<ActionResult<ApiResponseDto<CategoryResponseDto>>> GetCategoryById(int categoryId)
        {
            try
            {
                var category = await _context.Categories
                    .Where(c => c.Id == categoryId)
                    .Select(c => new CategoryResponseDto
                    {
                        Id = c.Id,
                        CategoryName = c.CategoryName,
                        Type = c.Type
                    })
                    .FirstOrDefaultAsync();

                if (category == null)
                {
                    return NotFound(ApiResponseDto<CategoryResponseDto>.ErrorResponse("Category not found"));
                }

                return Ok(ApiResponseDto<CategoryResponseDto>.SuccessResponse(category));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category {CategoryId}", categoryId);
                return StatusCode(500, ApiResponseDto<CategoryResponseDto>.ErrorResponse("An error occurred while retrieving the category"));
            }
        }
    }
}