using Serilog;
using Serilog.Sinks.MSSqlServer;

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
                        .WriteTo.MSSqlServer("server=localhost\\sqlexpress;database=superherodb;trusted_connection=true",
                            new MSSqlServerSinkOptions
                            {
                                TableName = "Logs",
                                SchemaName = "dbo",
                                AutoCreateSqlTable = true
                            })
                        .CreateLogger();



            log.Information(message);

        }
    }
}
