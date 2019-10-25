using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/*
 * 参考：
 *   https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-3.0
 *   https://blog.stephencleary.com/2018/05/microsoft-extensions-logging-part-1-introduction.html
 *   https://andrewlock.net/using-dependency-injection-in-a-net-core-console-application/
 *   
 *   Logging in .NET Core Console Applications(aslo with background task)
 *   https://blog.bitscry.com/2017/05/31/logging-in-net-core-console-applications/
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
        private readonly ILogger<FooService> _logger;
        public FooService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<FooService>();
        }

        public void DoThing(int number)
        {
            _logger.LogInformation($"Doing the thing {number}");
        }
    }

    public class Program
    {
        private static void Main()
        {
            // Create service collection
            IServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider serviceProvider = services.BuildServiceProvider();

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
