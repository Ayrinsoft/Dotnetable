var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// SPA-style fallback so deep links work when opening a docs page directly.
app.MapFallbackToFile("index.html");

app.Run();
