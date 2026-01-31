using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BookStore.Client;
using BookStore.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient to use the API base address
// In development, the API runs on a different port
var apiBaseAddress = builder.Configuration["ApiBaseAddress"] 
    ?? "http://localhost:5029";

builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(apiBaseAddress) 
});

// Register API client
builder.Services.AddScoped<IBookApiClient, BookApiClient>();

await builder.Build().RunAsync();
