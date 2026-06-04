using Microsoft.EntityFrameworkCore;
using ProductApi;

var builder = WebApplication.CreateBuilder(args);

// Read connection string from configuration / environment variable
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddControllers();

// Allow the Angular app to call this API
const string CorsPolicy = "AllowAngular";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Apply migrations / create database automatically on startup, with retry
// (the MySQL container may not be ready the instant the API starts)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var retries = 10;
    while (retries > 0)
    {
        try
        {
            db.Database.EnsureCreated();
            SeedData(db);
            break;
        }
        catch (Exception ex)
        {
            retries--;
            Console.WriteLine($"DB not ready ({ex.Message}). Retries left: {retries}");
            Thread.Sleep(5000);
        }
    }
}

app.UseCors(CorsPolicy);
app.MapControllers();

app.Run();

// Seed a couple of rows so the UI isn't empty on first run
static void SeedData(AppDbContext db)
{
    if (!db.Products.Any())
    {
        db.Products.AddRange(
            new Product { Name = "Keyboard", Description = "Mechanical keyboard", Price = 2499.00m },
            new Product { Name = "Mouse", Description = "Wireless mouse", Price = 999.00m }
        );
        db.SaveChanges();
    }
}
