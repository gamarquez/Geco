using Business;
using Contracts;
using Data;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// CONFIGURACI�N DE SERVICIOS
// ==========================================

// Agregar soporte para MVC
builder.Services.AddControllersWithViews();

// Configurar sesiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de expiraci�n
    options.Cookie.HttpOnly = true; // Seguridad
    options.Cookie.IsEssential = true; // Necesario para GDPR
    options.Cookie.Name = ".GECO.Session";
});

// Configurar HttpContextAccessor para acceder al contexto HTTP
builder.Services.AddHttpContextAccessor();

// ==========================================
// INYECCI�N DE DEPENDENCIAS - CAPAS
// ==========================================

// Data Layer
builder.Services.AddScoped<UsuarioData>();
builder.Services.AddScoped<ProfesionalData>();
builder.Services.AddScoped<ObraSocialData>();
builder.Services.AddScoped<PlanData>();
builder.Services.AddScoped<PacienteData>();
builder.Services.AddScoped<HistoriaClinicaData>();

// Business Layer (con sus contratos)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfesionalService, ProfesionalService>();
builder.Services.AddScoped<IObraSocialService, ObraSocialService>();
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<IPacienteService, PacienteService>();
builder.Services.AddScoped<IHistoriaClinicaService, HistoriaClinicaService>();

// Add services to the container.
builder.Services.AddRazorPages();

// ==========================================
// CONSTRUCCI�N DE LA APLICACI�N
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
