namespace ConsoleAppExample
{
    using System.Data.Common;
    using System.Data.SQLite;
    using System.Net;
    using Dapper;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Debug;
    using StackExchange.Contrib.Profiling.Storage;
    using StackExchange.Profiling;
    using StackExchange.Profiling.Data;

    internal class Program
    {
        private static void Main(string[] args)
        {
            MiniProfiler.Configure(new MiniProfilerOptions
            {
                Storage = new LoggingProvider(new DebugLoggerProvider().CreateLogger(nameof(ConsoleAppExample)),
                    LogLevel.Debug)
            });

            var mp = MiniProfiler.StartNew("Test");

            using (mp.Step("Level 1"))
            using (var conn = GetConnection())
            {
                conn.Query<long>("select 1");

                using (mp.Step("Level 2"))
                {
                    conn.Query<long>("select 1");
                }

                using (var wc = new WebClient())
                using (mp.CustomTiming("http", "GET https://google.com"))
                {
                    wc.DownloadString("https://google.com");
                }
            }

            mp.Stop();
        }

        public static DbConnection GetConnection()
        {
            DbConnection cnn = new SQLiteConnection("Data Source=:memory:");

            // to get profiling times, we have to wrap whatever connection we're using in a ProfiledDbConnection
            // when MiniProfiler.Current is null, this connection will not record any database timings
            if (MiniProfiler.Current != null)
            {
                cnn = new ProfiledDbConnection(cnn, MiniProfiler.Current);
            }

            cnn.Open();
            return cnn;
        }
    }
}