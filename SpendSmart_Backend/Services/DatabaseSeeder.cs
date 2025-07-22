using Microsoft.Extensions.DependencyInjection;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpendSmart_Backend
{
    public static class DatabaseSeeder
    {
        public static void Seed(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Seed categories if they don't exist
            if (!context.Categories.Any())
            {
                Console.WriteLine("Seeding categories...");
                
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
                context.Categories.AddRange(expenseCategories);
                context.Categories.AddRange(incomeCategories);
                
                try
                {
                    context.SaveChanges();
                    Console.WriteLine("Categories seeded successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error seeding categories: {ex.Message}");
                }
            }

            // Seed user with ID 1 if no users exist
            if (!context.Users.Any())
            {
                try
                {
                    // Check if the Users table has CreatedAt and UpdatedAt columns
                    bool hasCreatedAtColumn = false;
                    bool hasUpdatedAtColumn = false;
                    
                    try
                    {
                        // Try to query using these columns to check if they exist
                        context.Database.ExecuteSqlRaw("SELECT CreatedAt FROM Users WHERE 1=0");
                        hasCreatedAtColumn = true;
                    }
                    catch
                    {
                        // Column doesn't exist
                    }
                    
                    try
                    {
                        context.Database.ExecuteSqlRaw("SELECT UpdatedAt FROM Users WHERE 1=0");
                        hasUpdatedAtColumn = true;
                    }
                    catch
                    {
                        // Column doesn't exist
                    }

                    // Create user with ID 1
                    var user = new User
                    {
                        Id = 1,
                        UserName = "admin",
                        Password = "password123",
                        FirstName = "Admin",
                        LastName = "User",
                        Email = "admin@spendsmart.com",
                        Currency = "USD"
                    };

                    // Only set these properties if the columns exist
                    if (hasCreatedAtColumn)
                    {
                        user.GetType().GetProperty("CreatedAt")?.SetValue(user, DateTime.UtcNow);
                    }
                    
                    if (hasUpdatedAtColumn)
                    {
                        user.GetType().GetProperty("UpdatedAt")?.SetValue(user, DateTime.UtcNow);
                    }

                    // Add the user and save changes
                    context.Users.Add(user);
                    context.SaveChanges();
                    Console.WriteLine("User with ID 1 seeded successfully");
                }
                catch (Exception ex)
                {
                    // If there's an error with CreatedAt/UpdatedAt, try direct SQL insertion
                    try
                    {
                        context.Database.ExecuteSqlRaw(
                            "SET IDENTITY_INSERT Users ON; " +
                            "INSERT INTO Users (Id, UserName, Password, FirstName, LastName, Email, Currency) " +
                            "VALUES (1, 'admin', 'password123', 'Admin', 'User', 'admin@spendsmart.com', 'USD'); " +
                            "SET IDENTITY_INSERT Users OFF;");
                        Console.WriteLine("User with ID 1 seeded successfully using SQL");
                    }
                    catch (Exception innerEx)
                    {
                        Console.WriteLine($"Failed to seed user: {innerEx.Message}");
                    }
                }
            }
            
            // Print out all categories for reference
            try
            {
                var categories = context.Categories.ToList();
                Console.WriteLine($"Available categories ({categories.Count}):");
                foreach (var category in categories)
                {
                    Console.WriteLine($"ID: {category.Id}, Name: {category.CategoryName}, Type: {category.Type}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing categories: {ex.Message}");
            }
        }
    }
}
