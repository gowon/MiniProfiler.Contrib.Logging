using System;
using System.Data.Common;
using System.Data.SQLite;
using System.Net;
using Dapper;
using Microsoft.Extensions.Logging;
using StackExchange.Contrib.Profiling.Storage;
using StackExchange.Profiling;
using StackExchange.Profiling.Data;

namespace ConsoleAppExample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
                builder
                    .AddConsole()
                    .AddDebug()
                    .SetMinimumLevel(LogLevel.Trace));

            MiniProfiler.Configure(new MiniProfilerOptions
            {
                Storage = new LoggerStorage(loggerFactory, LogLevel.Debug)
            });

            var profiler = MiniProfiler.StartNew("Sample Profile");

            using (profiler.Step("Many operations"))
            using (var conn = GetConnection(profiler))
            {
                conn.Query<long>("select 1");

                using (profiler.Step("Nested operation"))
                {
                    conn.Query<long>("select 1");
                }

                using (var wc = new WebClient())
                using (profiler.CustomTiming("http", "GET https://google.com"))
                {
                    wc.DownloadString("https://google.com");
                }
            }

            profiler.Stop();

            Console.ReadKey();
        }

        public static DbConnection GetConnection(MiniProfiler profiler)
        {
            var connection = new SQLiteConnection("Data Source=:memory:");

            // to get profiling times, we have to wrap whatever connection we're using in a ProfiledDbConnection
            // when profiler is null, this connection will not record any database timings
            var profiled = new ProfiledDbConnection(connection, profiler);
            profiled.Open();
            return profiled;
        }
    }
}