using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.Services;

using System.Text;
using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// 2. Swagger setup
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. Database Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SpendSmartDb")));
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Your React dev server
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IRecurringTransactionService, RecurringTransactionService>();
builder.Services.AddHostedService<RecurringTransactionBackgroundService>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(

                "http://localhost:3000", "http://localhost:5173", "http://localhost:5174",
                "http://localhost:5175", "http://localhost:5176", "http://localhost:5177",
                "https://localhost:3000", "https://localhost:5173", "https://localhost:5174",
                "https://localhost:5175", "https://localhost:5176", "https://localhost:5177"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();

    });
});

// 5. Scoped Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EmailService>();

// 6. JWT Authentication Setup
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


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Optional: Enable HTTPS redirection in production
// if (!app.Environment.IsDevelopment())
// {
//     app.UseHttpsRedirection();
// }

app.UseCors("AllowReactApp");

app.UseAuthentication();

app.UseCors("AllowReact");


app.UseAuthorization();

app.MapControllers();

app.Run();
