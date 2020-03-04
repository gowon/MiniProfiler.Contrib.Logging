namespace StackExchange.Contrib.Profiling.Storage
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging.Abstractions;
    using Serilog;
    using Serilog.Extensions.Logging;
    using Serilog.Sinks.InMemory;
    using Serilog.Sinks.InMemory.Assertions;
    using StackExchange.Profiling;
    using Xunit;

    public class LoggingProviderTests
    {
        private static void TestMultiThreaded(MiniProfiler profiler)
        {
            static void DoWork()
            {
                Thread.Sleep(new Random().Next(1, 50));
            }

            using (profiler.Step("outer"))
            {
                Parallel.For(0, 5, i =>
                {
                    DoWork();

                    using (profiler.Step("step " + i))
                    {
                        using (profiler.CustomTiming("MyCustom", "test command"))
                        {
                            DoWork();
                        }

                        using (profiler.Step("sub-step" + i))
                        {
                            DoWork();
                        }
                    }
                });
            }
        }

        [Fact]
        public async Task OtherMethods()
        {
            MiniProfiler.Configure(new MiniProfilerOptions
            {
                Storage = new LoggingProvider(NullLogger.Instance)
            });

            var profiler = MiniProfiler.StartNew(nameof(Save));

            TestMultiThreaded(profiler);

            profiler.Stop();

            Assert.Null(profiler.Storage.Load(Guid.Empty));
            Assert.Null(await profiler.Storage.LoadAsync(Guid.Empty));
            Assert.Empty(profiler.Storage.List(0));
            Assert.Empty(await profiler.Storage.ListAsync(0));
            Assert.Empty(profiler.Storage.GetUnviewedIds(string.Empty));
            Assert.Empty(await profiler.Storage.GetUnviewedIdsAsync(string.Empty));
            profiler.Storage.SetViewed(string.Empty, Guid.Empty);
            Assert.True(profiler.Storage.SetViewedAsync(string.Empty, Guid.Empty).IsCompleted);
            profiler.Storage.SetUnviewed(string.Empty, Guid.Empty);
            Assert.True(profiler.Storage.SetUnviewedAsync(string.Empty, Guid.Empty).IsCompleted);
        }

        [Fact]
        public void Save()
        {
            var logger = new SerilogLoggerProvider(new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.InMemory()
                .CreateLogger()).CreateLogger(nameof(MiniProfiler));

            MiniProfiler.Configure(new MiniProfilerOptions
            {
                Storage = new LoggingProvider(logger)
            });

            var profiler = MiniProfiler.StartNew(nameof(Save));

            TestMultiThreaded(profiler);

            var result = profiler.Stop();

            Assert.True(result);

            InMemorySink.Instance
                .Should()
                .HaveMessage(profiler.RenderPlainText());
        }

        [Fact]
        public async Task SaveAsync()
        {
            var logger = new SerilogLoggerProvider(new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.InMemory()
                .CreateLogger()).CreateLogger(nameof(MiniProfiler));

            MiniProfiler.Configure(new MiniProfilerOptions
            {
                Storage = new LoggingProvider(logger)
            });

            var profiler = MiniProfiler.StartNew(nameof(SaveAsync));

            TestMultiThreaded(profiler);

            var result = await profiler.StopAsync();

            Assert.True(result);

            InMemorySink.Instance
                .Should()
                .HaveMessage(profiler.RenderPlainText());
        }

        [Fact]
        public void SaveWithLogLevelUnderMinimum()
        {
            var logger = new SerilogLoggerProvider(new LoggerConfiguration()
                .MinimumLevel.Warning()
                .WriteTo.InMemory()
                .CreateLogger()).CreateLogger(nameof(MiniProfiler));

            MiniProfiler.Configure(new MiniProfilerOptions
            {
                Storage = new LoggingProvider(logger)
            });

            var profiler = MiniProfiler.StartNew(nameof(Save));

            TestMultiThreaded(profiler);

            var result = profiler.Stop();

            Assert.True(result);

            InMemorySink.Instance
                .Should().Match(sink => !sink.LogEvents.Any());
        }
    }
}