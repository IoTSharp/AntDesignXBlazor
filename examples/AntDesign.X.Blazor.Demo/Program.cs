using AntDesign;
using AntDesign.X;
using AntDesign.X.Blazor.Demo;
using AntDesign.X.Blazor.Demo.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddAntDesign();
builder.Services.AddAntDesignX();
builder.Services.AddScoped<IXRequestClient, DemoMockXRequestClient>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
