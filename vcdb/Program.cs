using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using vcdb.CommandLine;
using vcdb.DependencyInjection;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;
using vcdb.Scripting.CheckConstraint;
using vcdb.Scripting.Collation;
using vcdb.Scripting.Column;
using vcdb.Scripting.Database;
using vcdb.Scripting.Index;
using vcdb.Scripting.Permission;
using vcdb.Scripting.PrimaryKey;
using vcdb.Scripting.Schema;
using vcdb.Scripting.Table;
using vcdb.Scripting.User;

[assembly: InternalsVisibleTo("vcdb.IntegrationTests")]

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
            services.InNamespace<ICheckConstraintComparer>().AddAsSingleton();
            services.InNamespace<ICollationComparer>().AddAsSingleton();
            services.InNamespace<IColumnComparer>().AddAsSingleton();
            services.InNamespace<IDatabaseComparer>().AddAsSingleton();
            services.InNamespace<IIndexComparer>().AddAsSingleton();
            services.InNamespace<IPrimaryKeyComparer>().AddAsSingleton();
            services.InNamespace<ISchemaComparer>().AddAsSingleton();
            services.InNamespace<ITableComparer>().AddAsSingleton();
            services.InNamespace<IUserComparer>().AddAsSingleton();
            services.InNamespace<INamedItemFinder>().AddAsSingleton();
            services.InNamespace<IPermissionComparer>().AddAsSingleton();

            services.InNamespace<IExecutor>().AddAsSingleton();

            services.AddSingleton<IOutput, ConsoleOutput>();
            services.AddSingleton<IInput, Input>();

            var defaultSearchPaths = new[] { options.WorkingDirectory, Path.GetDirectoryName(typeof(Program).Assembly.Location) };
            var databaseInferfaceLoader = new DatabaseInterfaceLoader(options.AssemblySearchPaths.OrEmptyCollection().Concat(defaultSearchPaths));
            var databaseServicesInstaller = databaseInferfaceLoader.GetServicesInstaller(options.GetDatabaseVersion());
            databaseServicesInstaller.RegisterServices(services, options.GetDatabaseVersion());

            services.AddSingleton(new JsonSerializer
            {
                Converters =
                {
                    new StringEnumConverter(),
                    new ReferencedSubJsonConverter(options.WorkingDirectory)
                },
                ContractResolver = new MultipleJsonContractResolver(new JsonOutputContractResolver(), new OptOut.OptOutContractResover()),
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = options.DefaultValueOutput
            });
        }
    }
}
