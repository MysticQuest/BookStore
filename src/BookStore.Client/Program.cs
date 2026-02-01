using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BookStore.Client;
using BookStore.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseAddress = builder.Configuration["ApiBaseAddress"];
if (string.IsNullOrEmpty(apiBaseAddress) || apiBaseAddress == "/")
{
    apiBaseAddress = builder.HostEnvironment.BaseAddress;
}

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseAddress)
});

builder.Services.AddScoped<IBookApiClient, BookApiClient>();
builder.Services.AddScoped<IOrderApiClient, OrderApiClient>();

await builder.Build().RunAsync();
