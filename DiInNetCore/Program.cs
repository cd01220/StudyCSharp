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
 *   https://blog.bitscry.com/2017/05/31/logging-in-net-core-console-applications/
 *   https://ballardsoftware.com/adding-appsettings-json-configuration-to-a-net-core-console-application/
 */
namespace DiInNetCore
{
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

            IConfigurationRoot configuration = serviceProvider.GetService<IConfigurationRoot>();
            MySettingsConfig mySettingsConfig = new MySettingsConfig();
            configuration.GetSection("MySettings").Bind(mySettingsConfig);

            Console.WriteLine("Hello World!");
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Add logging
            _ = services.AddLogging(builder =>
              {
                  _ = builder.AddFilter("Program", LogLevel.Debug)
                             .AddConsole()
                             .AddDebug();
              });

            // Build configuration
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            IConfigurationRoot configuration = builder.Build();
            services.AddSingleton(configuration);            
        }

        public class MySettingsConfig
        {
            public string AccountName { get; set; }
            public string ApiKey { get; set; }
            public string ApiSecret { get; set; }
        }
    }
}
