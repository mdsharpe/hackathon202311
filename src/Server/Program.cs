using Server.Hubs;
using Server.Services;
using Shared.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddHostedService<GameEngine>()
    .AddSingleton<GameBoard>();

builder.Services.AddSignalR(configure =>
{
#if DEBUG
    configure.EnableDetailedErrors = true;
#endif
}).AddMessagePackProtocol();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHttpsRedirection();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapHub<GameHub>("/gamehub");
app.MapFallbackToFile("index.html");

app.Run();
