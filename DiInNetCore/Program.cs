using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/*
 * 参考：
 *   https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-3.0
 *      
 *   Adding AppSettings.json Configuration to a .NET Core Console Application
 *   https://ballardsoftware.com/adding-appsettings-json-configuration-to-a-net-core-console-application/
 *   
 *   Background tasks with hosted services in ASP.NET Core
 *   https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-3.0&tabs=visual-studio
 *   
 *   Hosting services in .NET Core console application
 *   https://thinkrethink.net/2018/08/02/hostbuilder-ihost-ihostedserice-console-application/
 */
namespace DiInNetCore
{
    public interface IFooService
    {
        void DoThing(int number);
    }

    public class FooService : IFooService
    {
        private readonly ILogger<FooService> logger;
        public FooService(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<FooService>();
        }

        public void DoThing(int number)
        {
            logger.LogInformation($"Doing the thing {number}");
        }
    }

    public class Program
    {
        private static void Main()
        {
            // Create service collection
            IServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            ILogger logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger("Program");
            logger.LogInformation("Example log message");

            IFooService fooService = serviceProvider.GetService<IFooService>();
            fooService.DoThing(1);
            logger.LogInformation("Hello World!");
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Build configuration
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            IConfigurationRoot configuration = builder.Build();

            LogLevelSetting logLevelSetting = new LogLevelSetting();
            configuration.GetSection("LogLevelSettings").Bind(logLevelSetting);
            // Add logging
            _ = services.AddLogging(builder =>
                {
                    _ = builder.AddFilter("Program", Enum.Parse<LogLevel>(logLevelSetting.Program))
                               .AddFilter("DiInNetCore.FooService", Enum.Parse<LogLevel>(logLevelSetting.FooService))
                               .AddConsole()
                               .AddDebug();
                })
                .AddSingleton<IFooService, FooService>()
                .AddSingleton(configuration);             
        }

        private class LogLevelSetting
        {
            public string Program    { get; set; }
            public string FooService { get; set; }
        }
    }
}
