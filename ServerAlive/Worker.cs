using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using System;

namespace ServerAlive
{
    public class Worker : BackgroundService
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Worker));

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _log.Info(":Start:");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    Service.Start();
                }
                catch(Exception ex)
                {
                    Service.HttpConnect.StopListen();
                    _log.Error(ex);
                }
                
                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}
