using CandidateInfoAPI.Data;
using CandidateInfoAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
    
// Add services to the container.
builder.Services.AddControllers();            // API Controllers
builder.Services.AddEndpointsApiExplorer();   // API metadata for tools like Swagger
builder.Services.AddSwaggerGen();             // Swagger documentation generator

// Database configuration with Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
   
// Custom service registration
builder.Services.AddScoped<CandidateScraper>();

var app = builder.Build();

// Conditional middleware (only in development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();        // Enable Swagger endpoint 
    app.UseSwaggerUI();      // Enable Swagger UI
}

app.UseHttpsRedirection();   // Redirect HTTP to HTTPS
app.UseAuthorization();      // Enable authorization
app.MapControllers();        // Set up routing for controllers
app.Run();
