using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WorkerService
{
    // Introducing IHostLifetime and untangling the Generic Host startup interactions
    // https://andrewlock.net/introducing-ihostlifetime-and-untangling-the-generic-host-startup-interactions/
    public static class Program
    {
        public static void Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();
            host.Run();
        }

        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-3.0
        // If the app uses Entity Framework Core, don't change the name or signature of the CreateHostBuilder method. 
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            IHostBuilder builder = Host.CreateDefaultBuilder(args)
                       .ConfigureServices((hostContext, services) =>
                       {
                           _ = services.AddHostedService<Worker>();
                       });
            return builder;
        }
    }
}
