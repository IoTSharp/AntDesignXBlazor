using AntDesign;
using AntDesign.X;
using AntDesign.X.Blazor.Demo;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),
});

builder.Services.AddAntDesign();
builder.Services.AddAntDesignX();
builder.Services.AddScoped<IXRequestClient, DemoMockXRequestClient>();
builder.Services.AddScoped<DemoThemeState>();

await builder.Build().RunAsync();
