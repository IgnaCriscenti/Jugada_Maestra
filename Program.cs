using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;
using Solutaris.InfoWARE.ProtectedBrowserStorage.Extensions;
using TableroApuestas.Data;


var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json");



builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddScoped<ProtectedSessionStorage>();

builder.Services.AddScoped(sp =>
{
    // Obtiene la cadena de conexión desde la configuración
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("MiConexion");

    // Verifica que la cadena de conexión no sea nula o vacía
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("La cadena de conexión es nula o vacía. Verifica la configuración.");
    }

    // Crea una instancia de AccesoDatos con la cadena de conexión
    return new AccesoDatos(connectionString);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

using (var scope = app.Services.CreateScope())
{
    var acceso = scope.ServiceProvider.GetRequiredService<AccesoDatos>();
    await acceso.EnsureSchemaAsync();
}

app.Run();
