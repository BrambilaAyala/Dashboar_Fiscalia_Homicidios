using System;
using Microsoft.EntityFrameworkCore;
using ProyectoFisca.Data;
using ProyectoFisca.DataAccess;
using ProyectoFisca.Sources.Interface;
using ProyectoFisca.Sources.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(options =>
{
    options.RootDirectory = "/Page";
});

// Add HTTP Client for C# download pipeline
builder.Services.AddHttpClient();

// Register DataAccess Layer (sin quemar conexión directa en controladores o servicios)
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<IHomicidiosDataAccess, HomicidiosDataAccess>();

// Add MySQL database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("FiscaliaDatabase") 
        ?? throw new InvalidOperationException("Cadena de conexión 'FiscaliaDatabase' no encontrada en appsettings.json.")));

// Register C# Pipeline & Forensic Services (Sources/Interface & Sources/Services)
builder.Services.AddSingleton<INormalizadorGeoService, NormalizadorGeoService>();
builder.Services.AddScoped<IPdfPipelineService, PdfPipelineService>();
builder.Services.AddScoped<IEtlOrchestratorService, EtlOrchestratorService>();
builder.Services.AddScoped<IHomicidiosService, HomicidiosService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
        policy.WithOrigins("http://localhost:44497", "https://localhost:44497", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

var app = builder.Build();

// Auto-crear base de datos y tablas si no existen
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();
        
        // Inicializar catálogos en memoria
        var normalizador = scope.ServiceProvider.GetRequiredService<INormalizadorGeoService>();
        normalizador.Inicializar();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("No se pudo conectar a la base de datos al inicio: {Message}", ex.Message);
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();
app.UseAuthorization();

// Registrar endpoints de Razor Pages (.cshtml) y Controllers
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.Run();