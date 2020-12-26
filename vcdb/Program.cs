using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using vcdb.SqlServer;

namespace vcdb
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed(o =>
                   {
                       var outputFactory = new OutputFactory();
                       outputFactory.SetActualConsoleOutput(Console.Out);
                       Console.SetOut(Console.Error); //replace the ConsoleOutput so that all <loggger> messages go to STDERR rather than STDOUT // TODO: Replace this with CONOUT$

                       var serviceCollection = new ServiceCollection();
                       serviceCollection
                        .AddLogging(builder => builder.AddSimpleConsole(o => o.SingleLine = true))
                        .Configure<LoggerFilterOptions>(opts => opts.MinLevel = LogLevel.Information);

                       serviceCollection.AddSingleton<IOutputFactory>(outputFactory);
                       serviceCollection.AddSingleton(o);
                       ConfigureServices(serviceCollection);
                       var serviceProvider = serviceCollection.BuildServiceProvider();
                       var executor = serviceProvider.GetRequiredService<IExecutor>();

                       executor.Execute().Wait();
                   });
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<IExecutor, Executor>();
            services.AddSingleton<IConnectionFactory, ConnectionFactory>();
            services.AddSingleton<IDatabaseRepository, DatabaseRepository>();
            services.AddSingleton<ITableRepository, SqlServerTableRepository>();
            services.AddSingleton<IOutput, ConsoleOutput>();

            services.AddSingleton(new JsonSerializer
            {
                Converters =
                {
                    new StringEnumConverter()
                },
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            });
        }
    }
}
