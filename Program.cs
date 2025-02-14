using ProyectoFisca.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add MySQL database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("FiscaliaDatabase")));

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
        builder.WithOrigins("http://localhost:44497") // Cambia esto al dominio de tu app React
               .AllowAnyMethod() // Permitir todos los métodos (GET, POST, etc.)
               .AllowAnyHeader() // Permitir todos los encabezados HTTP
               .AllowCredentials()); // Permitir cookies o credenciales si son necesarias
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Apply CORS middleware
app.UseCors("AllowSpecificOrigin");

app.UseAuthentication(); // Si estás usando autenticación
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();