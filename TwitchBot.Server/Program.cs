using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TwitchBot.Server
{
    public static class Program
    {
        public async static Task Main(string[] args)
        {
            await CreateWebHostBuilder(args).Build().RunAsync();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                 .UseKestrel()
                 .UseContentRoot(Directory.GetCurrentDirectory())
                 .ConfigureAppConfiguration((hostingContext, config) =>
                 {
                     config
                         .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                         .AddJsonFile("appsettings.json", false, true)
                         .AddJsonFile($"appsettings.{ hostingContext.HostingEnvironment.EnvironmentName }.json", true, true)
                         .AddJsonFile("appsettings.local.json", true, true)
                         .AddEnvironmentVariables();
                 })
                 .ConfigureLogging((WebHostBuilderContext hostingContext, ILoggingBuilder logging) =>
                 {
                     var section = hostingContext.Configuration.GetSection("Logging");

                     logging.AddConfiguration(section);
                     logging.AddConsole();
                     logging.AddDebug();
                     logging.AddEventSourceLogger();

                     if (section.GetSection("LogToFile").Get<bool?>() == true)
                     {
                         logging.AddFile(section);
                     }
                 })
                 .UseStartup<Startup>()
                 .UseIIS();
        }
    }
}