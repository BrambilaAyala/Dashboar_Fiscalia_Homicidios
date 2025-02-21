using ProyectoFisca.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;

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

//Ejecucion en segundo plano
Task.Run(() => Ejecutar_Scripts());

app.Run();

//Ejecutar los scripts de python en segundo plano
static void Ejecutar_Scripts()
{
    string[] scripts = new string[]
    {
        "C:\\Users\\leona\\Documents\\ProyectoFisca\\Scripts\\DescargarPdf.py",
        "C:\\Users\\leona\\Documents\\ProyectoFisca\\Scripts\\Process_pdf.py",
        "C:\\Users\\leona\\Documents\\ProyectoFisca\\Scripts\\Normalizar_csv.py",
        "C:\\Users\\leona\\Documents\\ProyectoFisca\\Scripts\\app.py"
    };

    foreach(var script in scripts)
    {
        //Task.Run(() => Pre_Procesamiento(script));
        Pre_Procesamiento(script);
    }
}

//Ejecutar los script
static void Pre_Procesamiento(string scriptpath)
{
    try
    {
        string pythonPath = "C:\\Users\\leona\\AppData\\Local\\Microsoft\\WindowsApps\\pythonw3.11.exe";

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = pythonPath,
            Arguments = $"\"{scriptpath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using(Process process = new Process {StartInfo = psi})
        {
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            Console.WriteLine("Salida del script:");
            Console.WriteLine("output");
            if(!string.IsNullOrEmpty(error))
            {
                Console.WriteLine("Errores:");
                Console.WriteLine(error);
            }
        }
    }
    catch(Exception ex)
    {
        Console.WriteLine($"Error al ejecutar el script: {ex.Message}");
    }
}