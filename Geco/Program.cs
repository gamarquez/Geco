using Business;
using Contracts;
using Data;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// CONFIGURACIÓN DE SERVICIOS
// ==========================================

// Agregar soporte para MVC
builder.Services.AddControllersWithViews();

// Configurar sesiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de expiración
    options.Cookie.HttpOnly = true; // Seguridad
    options.Cookie.IsEssential = true; // Necesario para GDPR
    options.Cookie.Name = ".GECO.Session";
});

// Configurar HttpContextAccessor para acceder al contexto HTTP
builder.Services.AddHttpContextAccessor();

// ==========================================
// INYECCIÓN DE DEPENDENCIAS - CAPAS
// ==========================================

// Data Layer
builder.Services.AddScoped<UsuarioData>();
builder.Services.AddScoped<ProfesionalData>();
builder.Services.AddScoped<ObraSocialData>();
builder.Services.AddScoped<PlanData>();
builder.Services.AddScoped<PacienteData>();

// Business Layer (con sus contratos)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfesionalService, ProfesionalService>();
builder.Services.AddScoped<IObraSocialService, ObraSocialService>();
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<IPacienteService, PacienteService>();

// Add services to the container.
builder.Services.AddRazorPages();

// ==========================================
// CONSTRUCCIÓN DE LA APLICACIÓN
// ==========================================
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// IMPORTANTE: Habilitar sesiones antes de Authorization
app.UseSession();

app.UseAuthorization();

//app.MapRazorPages();

// Configurar rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
