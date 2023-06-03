using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using log4net.Config;
using System;
using System.IO;

namespace ServerAlive
{
    public class Program
    {
        public static void Main(string[] args)
        {
            XmlConfigurator.Configure(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config")));
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                })
                .UseWindowsService();
    }
}
