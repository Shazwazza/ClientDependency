using Serilog;
using System;
using System.Web;
using System.Web.Hosting;

namespace ClientDependency.Web.Test
{
    public class TestLogger : ClientDependency.Core.Logging.ILogger
    {
        public TestLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(HostingEnvironment.MapPath("~/App_Data/logs/cdfsite.txt"), rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("App started");
        }

        public void Debug(string msg)
        {
            Log.Logger.Debug(msg);
        }

        public void Info(string msg)
        {
            Log.Logger.Information(msg);
        }

        public void Warn(string msg)
        {
            Log.Logger.Warning(msg);
        }

        public void Error(string msg, Exception ex)
        {
            Log.Logger.Error(ex, msg);
        }

        public void Fatal(string msg, Exception ex)
        {
            Log.Logger.Fatal(ex, msg);
        }
    }
}