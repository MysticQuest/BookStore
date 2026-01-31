using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BookStore.Client;
using BookStore.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var hostAddress = builder.HostEnvironment.BaseAddress;
var isLocalhost = hostAddress.Contains("localhost") || hostAddress.Contains("127.0.0.1");

var apiBaseAddress = isLocalhost
    ? builder.Configuration["ApiBaseAddress"] ?? "http://localhost:5029"
    : builder.Configuration["ProductionApiBaseAddress"] ?? hostAddress;

builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(apiBaseAddress) 
});

builder.Services.AddScoped<IBookApiClient, BookApiClient>();
builder.Services.AddScoped<IOrderApiClient, OrderApiClient>();

await builder.Build().RunAsync();
