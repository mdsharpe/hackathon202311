using Microsoft.Extensions.Internal;
using Server.Hubs;
using Server.Services;
using Shared.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddHostedService<GameEngine>()
    .AddSingleton<GameBoard>()
    .AddSingleton<GameHub>()
    .AddSingleton<GameLogic>()
    .AddSingleton<ISystemClock, SystemClock>()
    .AddSingleton<DirtyTracker>();

builder.Services.AddRazorPages();

builder.Services
    .AddSignalR(configure =>
    {
#if DEBUG
        configure.EnableDetailedErrors = true;
#endif
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseHttpsRedirection();
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapHub<GameHub>("/gamehub");
app.MapFallbackToPage("/Index");

app.Run();
