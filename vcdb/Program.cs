using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using vcdb.CommandLine;
using vcdb.Output;
using vcdb.Scripting;
using vcdb.SqlServer;

[assembly:InternalsVisibleTo("vcdb.IntegrationTests")]

namespace vcdb
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            new Parser(settings =>
            {
                settings.CaseInsensitiveEnumValues = true;
                settings.CaseSensitive = false;
            }).ParseArguments<Options>(args)
                   .WithParsedAsync(async options =>
                   {
                       var outputFactory = new OutputFactory();
                       outputFactory.SetActualConsoleOutput(Console.Out);
                       Console.SetOut(Console.Error); //replace the ConsoleOutput so that all <loggger> messages go to STDERR rather than STDOUT // TODO: Replace this with CONOUT$

                       await ExecuteAsync(
                           options,
                           outputFactory,
                           code => Environment.ExitCode = code,
                           Console.Error,
                           Console.Out,
                           Console.IsOutputRedirected);
                   }).Wait();
        }

        internal static async Task ExecuteAsync(
            Options options,
            IOutputFactory outputFactory,
            Action<int> setExitCode,
            TextWriter errorOutput,
            TextWriter standardOutput,
            bool isOutputRedirected,
            Action<IServiceCollection> modifyServices = null)
        {
            try
            {
                var serviceCollection = new ServiceCollection();
                serviceCollection
                 .AddLogging(builder => builder.AddSimpleConsole(o => o.SingleLine = true))
                 .Configure<LoggerFilterOptions>(opts => opts.MinLevel = LogLevel.Information);

                serviceCollection.AddSingleton(outputFactory);
                serviceCollection.AddSingleton(options);
                ConfigureServices(serviceCollection, options);
                modifyServices?.Invoke(serviceCollection);

                using (var serviceProvider = serviceCollection.BuildServiceProvider())
                {
                    var executor = serviceProvider.GetRequiredService<IExecutor>();

                    try
                    {
                        await executor.Execute();
                    }
                    catch (Exception exc)
                    {
                        var logger = serviceProvider.GetRequiredService<ILogger<object>>();
                        logger.LogError(exc, "Error executing vcdb process");

                        if (isOutputRedirected)
                        {
                            standardOutput.WriteLine($"Error executing vcdb process: {exc.Message}");
                        }

                        setExitCode(-2);
                    }

                    serviceProvider.GetRequiredService<ILoggerFactory>().Dispose();
                }
            }
            catch (Exception exc)
            {
                setExitCode(-1);
                errorOutput.WriteLine(exc.ToString());
            }
        }

        private static void ConfigureServices(ServiceCollection services, Options options)
        {
            services.AddSingleton<IExecutor, Executor>();
            services.AddSingleton<IConnectionFactory, ConnectionFactory>();
            services.AddSingleton<IOutput, ConsoleOutput>();
            services.AddSingleton<IInput, Input>();
            services.AddSingleton<IColumnComparer, ColumnComparer>();
            services.AddSingleton<ITableComparer, TableComparer>();
            services.AddSingleton<IIndexComparer, IndexComparer>();
            services.AddSingleton<IDatabaseComparer, DatabaseComparer>();
            services.AddSingleton<ISchemaComparer, SchemaComparer>();
            services.AddSingleton<ICollationComparer, CollationComparer>();
            services.AddSingleton<ICheckConstraintComparer, CheckConstraintComparer>();
            services.AddSingleton<INamedItemFinder, NamedItemFinder>();
            services.AddSingleton<IHashHelper, HashHelper>();

            var databaseServicesInstaller = GetDatabaseServicesInstaller(options);
            databaseServicesInstaller.RegisterServices(services);

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

        private static IServicesInstaller GetDatabaseServicesInstaller(Options options)
        {
            switch (options.DatabaseType)
            {
                case DatabaseType.SqlServer:
                    return new SqlServerInstaller();
            }

            throw new NotSupportedException($"Database type {options.DatabaseType} isn't currently supported");
        }
    }
}
