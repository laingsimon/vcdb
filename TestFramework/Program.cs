using CommandLine;
using JsonEqualityComparer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TestFramework
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed(o =>
                   {
                       var serviceCollection = new ServiceCollection();
                       serviceCollection
                        .AddLogging(builder => builder.AddSimpleConsole(o => o.SingleLine = true))
                        .Configure<LoggerFilterOptions>(opts => opts.MinLevel = o.MinLogLevel);

                       serviceCollection.AddSingleton(o);
                       ConfigureServices(serviceCollection);
                       var serviceProvider = serviceCollection.BuildServiceProvider();
                       var executor = serviceProvider.GetRequiredService<ITestFramework>();

                       executor.Execute().Wait();
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
