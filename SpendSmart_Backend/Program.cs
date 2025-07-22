using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.Services;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    // Ensure all HTTP methods are supported
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework - Use SQL Server database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SpendSmartDb")));

// Register custom services
builder.Services.AddScoped<BudgetService>();
builder.Services.AddScoped<TransactionService>();

// Add CORS policy for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:5174",
                             "http://localhost:5175", "http://localhost:5176", "http://localhost:5177",
                             "https://localhost:3000", "https://localhost:5173", "https://localhost:5174",
                             "https://localhost:5175", "https://localhost:5176", "https://localhost:5177")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Temporarily disable HTTPS redirection for testing
// app.UseHttpsRedirection();

// Use CORS policy
app.UseCors("AllowReactApp");

app.UseAuthorization();

app.MapControllers();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Check if user with ID 1 exists
        var userExists = context.Users.Any(u => u.Id == 1);
        
        if (!userExists)
        {
            // Try to insert user with ID 1 using raw SQL to avoid CreatedAt/UpdatedAt issues
            try
            {
                // Check if columns exist in the Users table
                bool hasCreatedAt = false;
                bool hasUpdatedAt = false;
                
                try
                {
                    context.Database.ExecuteSqlRaw("SELECT CreatedAt FROM Users WHERE 1=0");
                    hasCreatedAt = true;
                }
                catch (SqlException)
                {
                    // Column doesn't exist
                    logger.LogInformation("CreatedAt column doesn't exist in Users table");
                }
                
                try
                {
                    context.Database.ExecuteSqlRaw("SELECT UpdatedAt FROM Users WHERE 1=0");
                    hasUpdatedAt = true;
                }
                catch (SqlException)
                {
                    // Column doesn't exist
                    logger.LogInformation("UpdatedAt column doesn't exist in Users table");
                }
                
                // Insert user with ID 1
                string sql;
                if (hasCreatedAt && hasUpdatedAt)
                {
                    sql = "SET IDENTITY_INSERT Users ON; " +
                          "INSERT INTO Users (Id, UserName, Password, FirstName, LastName, Email, Currency, CreatedAt, UpdatedAt) " +
                          "VALUES (1, 'admin', 'password123', 'Admin', 'User', 'admin@spendsmart.com', 'USD', GETDATE(), GETDATE()); " +
                          "SET IDENTITY_INSERT Users OFF;";
                }
                else
                {
                    sql = "SET IDENTITY_INSERT Users ON; " +
                          "INSERT INTO Users (Id, UserName, Password, FirstName, LastName, Email, Currency) " +
                          "VALUES (1, 'admin', 'password123', 'Admin', 'User', 'admin@spendsmart.com', 'USD'); " +
                          "SET IDENTITY_INSERT Users OFF;";
                }
                
                context.Database.ExecuteSqlRaw(sql);
                logger.LogInformation("Successfully added user with ID 1");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to add user with ID 1 using SQL");
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();