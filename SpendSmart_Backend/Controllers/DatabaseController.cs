using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace SpendSmart_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DatabaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("check-categories")]
        public async Task<IActionResult> CheckCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            return Ok(new
            {
                TotalCount = categories.Count,
                ExpenseCategories = categories.Where(c => c.Type == "Expense").Count(),
                IncomeCategories = categories.Where(c => c.Type == "Income").Count(),
                Categories = categories.Select(c => new
                {
                    c.Id,
                    c.CategoryName,
                    c.Type,
                    c.Icon,
                    c.Color
                }).OrderBy(c => c.Type).ThenBy(c => c.CategoryName)
            });
        }

        [HttpGet("check-users")]
        public IActionResult CheckUsers()
        {
            try
            {
                // Use raw SQL to avoid the CreatedAt/UpdatedAt issue
                var users = _context.Database.SqlQuery<UserDto>($"SELECT Id, UserName, Email, FirstName, LastName, Currency FROM Users").ToList();
                
                return Ok(new
                {
                    TotalCount = users.Count,
                    Users = users
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message, Details = "Error retrieving users" });
            }
        }

        [HttpPost("seed-categories")]
        public async Task<IActionResult> SeedCategories()
        {
            if (await _context.Categories.AnyAsync())
            {
                return Ok(new { Message = "Categories already exist", Count = await _context.Categories.CountAsync() });
            }

            // Create expense categories
            var expenseCategories = new List<Category>
            {
                new Category { CategoryName = "Food", Type = "Expense", Icon = "🍔", Color = "#FF5733" },
                new Category { CategoryName = "Housing", Type = "Expense", Icon = "🏠", Color = "#3498DB" },
                new Category { CategoryName = "Transportation", Type = "Expense", Icon = "🚗", Color = "#2ECC71" },
                new Category { CategoryName = "Utilities", Type = "Expense", Icon = "💡", Color = "#9B59B6" },
                new Category { CategoryName = "Healthcare", Type = "Expense", Icon = "🏥", Color = "#E74C3C" },
                new Category { CategoryName = "Entertainment", Type = "Expense", Icon = "🎬", Color = "#F39C12" },
                new Category { CategoryName = "Shopping", Type = "Expense", Icon = "🛍️", Color = "#16A085" },
                new Category { CategoryName = "Education", Type = "Expense", Icon = "📚", Color = "#8E44AD" },
                new Category { CategoryName = "Personal Care", Type = "Expense", Icon = "💇", Color = "#D35400" },
                new Category { CategoryName = "Travel", Type = "Expense", Icon = "✈️", Color = "#2980B9" }
            };
                
            // Create income categories
            var incomeCategories = new List<Category>
            {
                new Category { CategoryName = "Salary", Type = "Income", Icon = "💰", Color = "#F1C40F" },
                new Category { CategoryName = "Freelance", Type = "Income", Icon = "💻", Color = "#27AE60" },
                new Category { CategoryName = "Investments", Type = "Income", Icon = "📈", Color = "#E67E22" },
                new Category { CategoryName = "Gifts", Type = "Income", Icon = "🎁", Color = "#C0392B" },
                new Category { CategoryName = "Other Income", Type = "Income", Icon = "💵", Color = "#1ABC9C" }
            };
                
            // Add all categories
            _context.Categories.AddRange(expenseCategories);
            _context.Categories.AddRange(incomeCategories);
            
            await _context.SaveChangesAsync();
            
            return Ok(new { 
                Message = "Categories seeded successfully", 
                Count = await _context.Categories.CountAsync(),
                Categories = await _context.Categories.Select(c => new { c.Id, c.CategoryName, c.Type }).ToListAsync()
            });
        }
        
        [HttpPost("seed-user")]
        public IActionResult SeedUser()
        {
            try
            {
                // Check if user with ID 1 exists
                bool userExists = false;
                try
                {
                    userExists = _context.Database.SqlQuery<bool>($"SELECT CASE WHEN EXISTS (SELECT 1 FROM Users WHERE Id = 1) THEN 1 ELSE 0 END").FirstOrDefault();
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { Error = ex.Message, Details = "Error checking if user exists" });
                }
                
                if (userExists)
                {
                    return Ok(new { Message = "User with ID 1 already exists" });
                }
                
                // Insert user with ID 1 using raw SQL
                try
                {
                    _context.Database.ExecuteSqlRaw(
                        "SET IDENTITY_INSERT Users ON; " +
                        "INSERT INTO Users (Id, UserName, Password, FirstName, LastName, Email, Currency) " +
                        "VALUES (1, 'admin', 'password123', 'Admin', 'User', 'admin@spendsmart.com', 'USD'); " +
                        "SET IDENTITY_INSERT Users OFF;");
                    
                    return Ok(new { Message = "User with ID 1 created successfully" });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { Error = ex.Message, Details = "Error creating user" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
        
        [HttpGet("fix-budget-schema")]
        public IActionResult FixBudgetSchema()
        {
            try
            {
                // Check if CategoryId column exists in Budgets table
                bool categoryIdExists = false;
                try
                {
                    _context.Database.ExecuteSqlRaw("SELECT CategoryId FROM Budgets WHERE 1=0");
                    categoryIdExists = true;
                }
                catch
                {
                    // Column doesn't exist
                }
                
                if (categoryIdExists)
                {
                    // Drop the column if it exists
                    _context.Database.ExecuteSqlRaw(
                        "IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'CategoryId' AND Object_ID = Object_ID('Budgets')) " +
                        "BEGIN " +
                        "    ALTER TABLE Budgets DROP COLUMN CategoryId " +
                        "END");
                    
                    return Ok(new { Message = "CategoryId column removed from Budgets table" });
                }
                
                return Ok(new { Message = "Budget schema is correct, no changes needed" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message, Details = "Error fixing budget schema" });
            }
        }

        [HttpGet("debug-budget-creation")]
        public IActionResult DebugBudgetCreation()
        {
            try
            {
                // Get all categories
                var categories = _context.Categories.ToList();
                
                // Get category IDs
                var categoryIds = categories.Select(c => c.Id).ToList();
                
                // Get budget categories
                var budgetCategories = _context.BudgetCategories.ToList();
                
                // Check for invalid category IDs in budget categories
                var invalidCategoryIds = budgetCategories
                    .Select(bc => bc.CategoryId)
                    .Where(id => !categoryIds.Contains(id))
                    .Distinct()
                    .ToList();
                
                return Ok(new
                {
                    TotalCategories = categories.Count,
                    AvailableCategoryIds = categoryIds,
                    TotalBudgetCategories = budgetCategories.Count,
                    InvalidCategoryIds = invalidCategoryIds,
                    Message = "When creating a budget, make sure to use only the category IDs listed in AvailableCategoryIds"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message, Details = "Error debugging budget creation" });
            }
        }
        
        [HttpPost("reset-categories")]
        public async Task<IActionResult> ResetCategories()
        {
            try
            {
                // Delete all existing categories
                _context.Database.ExecuteSqlRaw("DELETE FROM BudgetCategories");
                _context.Database.ExecuteSqlRaw("DELETE FROM Categories");
                
                // Create expense categories
                var expenseCategories = new List<Category>
                {
                    new Category { CategoryName = "Food", Type = "Expense", Icon = "🍔", Color = "#FF5733" },
                    new Category { CategoryName = "Housing", Type = "Expense", Icon = "🏠", Color = "#3498DB" },
                    new Category { CategoryName = "Transportation", Type = "Expense", Icon = "🚗", Color = "#2ECC71" },
                    new Category { CategoryName = "Utilities", Type = "Expense", Icon = "💡", Color = "#9B59B6" },
                    new Category { CategoryName = "Healthcare", Type = "Expense", Icon = "🏥", Color = "#E74C3C" },
                    new Category { CategoryName = "Entertainment", Type = "Expense", Icon = "🎬", Color = "#F39C12" },
                    new Category { CategoryName = "Shopping", Type = "Expense", Icon = "🛍️", Color = "#16A085" },
                    new Category { CategoryName = "Education", Type = "Expense", Icon = "📚", Color = "#8E44AD" },
                    new Category { CategoryName = "Personal Care", Type = "Expense", Icon = "💇", Color = "#D35400" },
                    new Category { CategoryName = "Travel", Type = "Expense", Icon = "✈️", Color = "#2980B9" }
                };
                
                // Create income categories
                var incomeCategories = new List<Category>
                {
                    new Category { CategoryName = "Salary", Type = "Income", Icon = "💰", Color = "#F1C40F" },
                    new Category { CategoryName = "Freelance", Type = "Income", Icon = "💻", Color = "#27AE60" },
                    new Category { CategoryName = "Investments", Type = "Income", Icon = "📈", Color = "#E67E22" },
                    new Category { CategoryName = "Gifts", Type = "Income", Icon = "🎁", Color = "#C0392B" },
                    new Category { CategoryName = "Other Income", Type = "Income", Icon = "💵", Color = "#1ABC9C" }
                };
                
                // Add all categories
                _context.Categories.AddRange(expenseCategories);
                _context.Categories.AddRange(incomeCategories);
                
                await _context.SaveChangesAsync();
                
                // Return the newly created categories with their IDs
                var newCategories = _context.Categories.Select(c => new { c.Id, c.CategoryName, c.Type }).ToList();
                
                return Ok(new
                {
                    Message = "Categories reset successfully",
                    Categories = newCategories
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message, Details = "Error resetting categories" });
            }
        }
    }
    
    // DTO for raw SQL query
    public class UserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Currency { get; set; }
    }
}
