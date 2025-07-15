using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.Services;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);



// Configure services
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SpendSmartDb")));

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

// Scoped Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EmailService>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

// ✅ Proper CORS Configuration
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Define named CORS policy


});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Temporarily disable HTTPS redirection for testing
// app.UseHttpsRedirection();

// Use CORS policy
app.UseCors("AllowReactApp");

// ✅ Use the exact same policy name here
app.UseCors("AllowLocalhost5173");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run(); 
