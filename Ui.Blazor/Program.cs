using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using Ui.Blazor;
using Ui.Blazor.Auth;
using Ui.Blazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

// Config: ApiBaseUrl
// appsettings.json (UI) nên có: { "Api": { "BaseUrl": "https://localhost:7070" } }
Uri uiBaseUri = new(builder.HostEnvironment.BaseAddress);
string defaultApiBaseUrl = uiBaseUri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)
    ? "https://localhost:7070"
    : "http://localhost:5294";

string apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? defaultApiBaseUrl;
apiBaseUrl = apiBaseUrl.Replace("0.0.0.0", "localhost", StringComparison.OrdinalIgnoreCase);

// Default HttpClient used by razor pages that @inject HttpClient.
// Without this, calling HttpClient with relative URLs (e.g. "/api/auth/login") throws:
// "An invalid request URI was provided... BaseAddress must be set."
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<ITokenStore, BrowserTokenStore>();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();
builder.Services.AddScoped<AppState>();
builder.Services.AddScoped<StorageInterop>();

builder.Services.AddScoped<AuthHeaderHandler>();

builder.Services.AddHttpClient<ApiClient>(client => { client.BaseAddress = new Uri(apiBaseUrl); })
    .AddHttpMessageHandler<AuthHeaderHandler>();

WebAssemblyHost app = builder.Build();

AppState state = app.Services.GetRequiredService<AppState>();
await state.InitializeAsync();

await app.RunAsync();
