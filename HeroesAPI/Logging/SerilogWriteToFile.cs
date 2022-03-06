using Serilog;

namespace HeroesAPI.Logging
{
    public static class SerilogWriteToFile
    {
        public static void LogingMethod(string message)
        {
            string fullPath = Environment.CurrentDirectory + @"\logs.txt";
            Serilog.Core.Logger? log = new LoggerConfiguration()
                     .MinimumLevel.Debug()
                     .WriteTo.File(fullPath, rollingInterval: RollingInterval.Day)
                     .CreateLogger();
            log.Information(message);
        }
    }
}
