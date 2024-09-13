using FastFoodAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FastFoodAPI.Services;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add CORS services to allow communication from the React app
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", builder =>
    {
        builder.WithOrigins("http://localhost:3000")  // Update with your frontend URL if needed
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials(); // Enable credentials if needed
    });
});

// Register UserService for dependency injection
builder.Services.AddScoped<IUserService, UserService>();

// Register the DbContext with MySQL connection string
builder.Services.AddDbContext<FastFoodAPIDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 37)));  // Update MySQL version if needed
});

// Add services for API and MVC controllers
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();

// Add DbContext and Identity services
builder.Services.AddDbContext<FastFoodAPIDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<FastFoodAPIDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Home/Login"; // Redirect to login if unauthorized
});

// Add Swagger/OpenAPI services
builder.Services.AddEndpointsApiExplorer();

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await IdentitySeed.SeedRolesAndAdminAsync(services); // Call your seeding function here
}

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Enable CORS middleware
app.UseCors("AllowReactApp");

// Enable Swagger for API documentation

{
    app.UseExceptionHandler("/Home/Error"); // Global error handling
    app.UseHsts(); // Enforce HTTPS Strict Transport Security
}

app.UseHttpsRedirection(); // Enforce HTTPS redirection
app.UseStaticFiles(); // Serve static files

app.UseRouting(); // Enable routing middleware

// Enable JWT Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map API controllers for API routes
app.MapControllers();

// Map default MVC route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



try
{
    app.Run(); // Start the application
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred while starting the application: {ex.Message}");
}
