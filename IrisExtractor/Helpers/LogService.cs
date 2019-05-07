using Serilog;
using Serilog.Core;

namespace ImageEditor.Helpers
{
    public static class LogService
    {
        private static readonly Logger _log  = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        static LogService()
        {
            _log.Information("Logging started");
        }
        public static void Write(string msg)
        {
            _log.Information(msg);
            _log.Debug(msg);
        }
    }
}
