using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Add database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SpendSmartDb")));

// Add email service
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

// Configure the HTTP request pipeline - order matters!
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use CORS before routing
app.UseCors("AllowAll");

// Use routing
app.UseRouting();

// Map controllers
app.MapControllers();

// Initialize database after configuration
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Console.WriteLine("Testing database connection...");
        
        // Test connection
        if (context.Database.CanConnect())
        {
            Console.WriteLine("Database connection successful!");
            
            try
            {
                // Try to apply pending migrations
                var pendingMigrations = context.Database.GetPendingMigrations();
                if (pendingMigrations.Any())
                {
                    Console.WriteLine($"Applying {pendingMigrations.Count()} pending migrations...");
                    context.Database.Migrate();
                    Console.WriteLine("Migrations applied successfully!");
                }
                else
                {
                    Console.WriteLine("Database is up to date!");
                }
            }
            catch (Exception migrationEx) when (migrationEx.Message.Contains("already an object named"))
            {
                Console.WriteLine("Tables already exist. Skipping migration...");
                Console.WriteLine("Database is ready to use!");
            }
        }
        else
        {
            Console.WriteLine("Cannot connect to database. Creating database...");
            context.Database.EnsureCreated();
        }
        
        Console.WriteLine("Database initialized successfully!");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Database initialization failed: {ex.Message}");
    Console.WriteLine("Application will continue without database functionality.");
}

Console.WriteLine("Starting server on http://localhost:5110");
app.Run("http://localhost:5110");
