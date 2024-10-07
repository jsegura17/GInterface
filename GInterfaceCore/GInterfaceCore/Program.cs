using GInterfaceCore.Components;
using GInterfaceCore.Interfaces;
using Radzen;
using Microsoft.Extensions.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
// Configura Serilog para escribir en un archivo
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("Logs/logfile.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Añadir Serilog como proveedor de logs
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddRadzenComponents();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole(); // Log en consola
    loggingBuilder.AddDebug();   // Log en debug
});

builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(GInterfaceCore.Client._Imports).Assembly);

try
{
    Log.Information("Iniciando la aplicación web");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación falló al iniciarse");
}
finally
{
    Log.CloseAndFlush();
}
app.Run();
