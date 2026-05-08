using AntDesign;
using AntDesign.X;
using AntDesign.X.Blazor.Demo;
using AntDesign.X.Blazor.Demo.Components;
using AntDesign.X.Blazor.Demo.Services;

DotEnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);

// 让 _content/<package>/... 这类 RCL static web assets 在所有环境（含 Production）都可被解析
builder.WebHost.UseStaticWebAssets();

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddAntDesign();
builder.Services.AddAntDesignX();
builder.Services.AddScoped<IXRequestClient, DemoMockXRequestClient>();
builder.Services.AddScoped<DemoThemeState>();

// DeepSeek 实时对话（API key 从 .env / 环境变量读取，不入库）
var deepSeekOptions = new DeepSeekOptions
{
    ApiKey = builder.Configuration["DEEPSEEK_API_KEY"]
        ?? builder.Configuration["DeepSeek:ApiKey"]
        ?? string.Empty,
    Endpoint = builder.Configuration["DEEPSEEK_ENDPOINT"]
        ?? builder.Configuration["DeepSeek:Endpoint"]
        ?? "https://api.deepseek.com/chat/completions",
    Model = builder.Configuration["DEEPSEEK_MODEL"]
        ?? builder.Configuration["DeepSeek:Model"]
        ?? "deepseek-chat",
};
builder.Services.AddSingleton(deepSeekOptions);
builder.Services.AddHttpClient("DeepSeek");
builder.Services.AddScoped<DeepSeekChatClient>();

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
