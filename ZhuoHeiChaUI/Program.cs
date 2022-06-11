using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using ZhuoHeiChaUI.Services;

namespace ZhuoHeiChaUI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            var backendAddress = builder.Configuration.GetValue<string>("BackendUrl");
            builder.Services.AddSingleton(new HttpClient
            {
                BaseAddress = new Uri(backendAddress)
            });
            builder.Services.AddSingleton<PlayerHubConnectionService>();
            builder.Services.AddSingleton<LocalEventService>();

            await builder.Build().RunAsync();
        }
    }
}