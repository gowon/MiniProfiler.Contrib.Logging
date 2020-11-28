# MiniProfilerContrib.Logging

[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/MiniProfilerContrib.Logging?color=blue)](https://www.nuget.org/packages/MiniProfilerContrib.Logging)
[![Nuget](https://img.shields.io/nuget/dt/MiniProfilerContrib.Logging?color=blue)](https://www.nuget.org/packages/MiniProfilerContrib.Logging)
![build](https://github.com/gowon/MiniProfilerContrib.Logging/workflows/build/badge.svg)
[![codecov](https://codecov.io/gh/gowon/MiniProfilerContrib.Logging/branch/master/graph/badge.svg)](https://codecov.io/gh/gowon/MiniProfilerContrib.Logging)

Save MiniProfiler results into a Microsoft.Extensions.Logging logger.

## Installing via NuGet

To get started install the *MiniProfilerContrib.Logging* package:

```powershell
PM> Install-Package MiniProfilerContrib.Logging
```

or

```bash
dotnet add package MiniProfilerContrib.Logging
```

## Usage

The `LoggerStorage` can accept an `ILoggerFactory` or `ILogger<MiniProfiler>`. These can be retrieved through dependency injection or by bootstrapping a logger factory.

```csharp
var loggerFactory = LoggerFactory.Create(builder =>
    builder
        .AddConsole()
        .AddDebug()
        .SetMinimumLevel(LogLevel.Trace));

var logger = loggerFactory.CreateLogger<MiniProfiler>()
```

Then, setup the `MiniProfiler` using a `LoggerStorage`, passing along the logger as well as the logging level the profiler output will be (default `LogLevel.Debug`).

```csharp
MiniProfiler.Configure(new MiniProfilerOptions
{
    Storage = new LoggerStorage(logger)
});
```

And you can continue to use MiniProfiler as usual.

> See the [MiniProfiler documentation](https://miniprofiler.com/) and the `samples` folder for working examples.

## License

MIT