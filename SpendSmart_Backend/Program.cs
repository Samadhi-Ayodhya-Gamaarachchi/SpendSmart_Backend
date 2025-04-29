var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // Needed for minimal APIs and Swagger
builder.Services.AddSwaggerGen(); // Generates Swagger docs

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();              // Generates the Swagger JSON
    app.UseSwaggerUI();           // Serves the Swagger UI
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
