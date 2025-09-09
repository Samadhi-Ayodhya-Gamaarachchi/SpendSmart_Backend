
﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.DTOs;


namespace SpendSmart_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetCategories")]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories([FromQuery] string? type)
        {
            var categories = _context.Categories.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(type))
            {
                categories = categories.Where(c => c.Type == type);
            }

            var result = await categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Type = c.Type,
                })
                .ToListAsync();

            return Ok(result);
        }
    }

}

