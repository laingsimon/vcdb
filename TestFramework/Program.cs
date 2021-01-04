using CommandLine;
using DiffPlex;
using DiffPlex.DiffBuilder;
using JsonEqualityComparer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace TestFramework
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            new Parser(settings =>
            {
                settings.CaseInsensitiveEnumValues = false;
                settings.CaseSensitive = false;
            }).ParseArguments<Options>(args)
                   .WithParsed(o =>
                   {
                       try
                       {
                           var serviceCollection = new ServiceCollection();
                           serviceCollection
                            .AddLogging(builder => builder.AddSimpleConsole(o => o.SingleLine = true))
                            .Configure<LoggerFilterOptions>(opts => opts.MinLevel = o.MinLogLevel);

                           serviceCollection.AddSingleton(o);
                           ConfigureServices(serviceCollection);
                           using (var serviceProvider = serviceCollection.BuildServiceProvider())
                           {
                               var executor = serviceProvider.GetRequiredService<ITestFramework>();

                               try
                               {
                                   executor.Execute().Wait();

                                   var context = serviceProvider.GetRequiredService<ExecutionContext>();
                                   Environment.ExitCode = context.Fail; //report the number of failures in the exit code
                               }
                               catch (Exception exc)
                               {
                                   Environment.ExitCode = -1;
                                   Console.Error.WriteLine(exc.ToString());
                               }
                           }
                       }
                       catch (Exception exc)
                       {
                            Environment.ExitCode = -2;
                            Console.Error.WriteLine(exc.ToString());
                       }
                   });
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<ITestFramework, TestFramework>();
            services.AddSingleton<ISql, Sql>();
            services.AddScoped<IJson, Json>();
            services.AddScoped<ScenarioDirectoryFactory>();
            services.AddScoped(factory => factory.GetRequiredService<ScenarioDirectoryFactory>().ScenarioDirectory);
            services.AddSingleton<ExecutionContext>();
            services.AddSingleton<IJsonEqualityComparer, Comparer>();
            services.AddSingleton<IInlineDiffBuilder>(InlineDiffBuilder.Instance);
            services.AddSingleton<IDocker, Docker>();

            services.AddSingleton(new JsonSerializer
            {
                Converters =
                {
                    new StringEnumConverter()
                }
            });

            services.AddScoped<IScenarioExecutor, ScenarioExecutor>();
        }
    }
}
