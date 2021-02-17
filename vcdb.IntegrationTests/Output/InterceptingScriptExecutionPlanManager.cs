using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using vcdb.Output;
using vcdb.Scripting.ExecutionPlan;

namespace vcdb.IntegrationTests.Output
{
    internal class InterceptingScriptExecutionPlanManager : IScriptExecutionPlanManager
    {
        private readonly ScriptExecutionPlanManager actualManager;
        private readonly Scenario scenario;
        private readonly IDatabaseProduct databaseProduct;

        public InterceptingScriptExecutionPlanManager(ScriptExecutionPlanManager actualManager, Scenario scenario, IDatabaseProduct databaseProduct)
        {
            this.actualManager = actualManager;
            this.scenario = scenario;
            this.databaseProduct = databaseProduct;
        }

        public ScriptExecutionPlan CreateExecutionPlan(IEnumerable<IScriptTask> tasks)
        {
            var tasksArray = tasks.ToArray();
            var plan = actualManager.CreateExecutionPlan(tasksArray);

            WritePlanToDisk(GetPlanDetails(tasksArray, plan));

            return plan;
        }

        private void WritePlanToDisk(PlanDetails planDetails)
        {
            var outputFilePath = Path.Combine(scenario.FullName, $"ExecutionPlan.{databaseProduct.Name}.json");
            File.WriteAllText(
                outputFilePath,
                JsonConvert.SerializeObject(
                    planDetails,
                    new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        Converters =
                        {
                            new StringEnumConverter(),
                            new TableComponentNameConverter()
                        },
                        NullValueHandling = NullValueHandling.Ignore
                    }));
        }

        private PlanDetails GetPlanDetails(IReadOnlyCollection<IScriptTask> tasksArray, ScriptExecutionPlan plan)
        {
            var ids = new Dictionary<IScriptTask, string>();

            return new PlanDetails
            {
                UnorderedTasks = ScriptTaskDetails.From(tasksArray, ids).ToArray(),
                OrderedTasks = ScriptTaskDetails.From(plan.tasks, ids).ToArray()
            };
        }

        private class PlanDetails
        {
            public int UnorderedTaskCount => UnorderedTasks.Count;
            public int OrderedTaskCount => OrderedTasks.Count;

            public string UnorderedIds => string.Join(" > ", UnorderedTasks.Select(t => t.Id));
            public string OrderedIds => string.Join(" > ", OrderedTasks.Select(t => t.Id));

            [JsonIgnore]
            public IReadOnlyCollection<ScriptTaskDetails> UnorderedTasks { get; set; }
            public IReadOnlyCollection<ScriptTaskDetails> OrderedTasks { get; set; }
        }

        private class ScriptTaskDetails
        {
            public string Id { get; set; }
            public IReadOnlyCollection<ScriptTaskDependency> Requires { get; set; }
            public IReadOnlyCollection<ScriptTaskDependency> Drops { get; set; }
            public IReadOnlyCollection<ScriptTaskDependency> CreatesOrAlters { get; set; }
            public string Script { get; set; }

            public static IEnumerable<ScriptTaskDetails> From(IEnumerable<IScriptTask> tasks, IDictionary<IScriptTask, string> ids)
            {
                foreach (var task in tasks)
                {
                    if (task is IEnumerable<IScriptTask> enumerable)
                    {
                        foreach (var subDetails in From(enumerable, ids))
                        {
                            yield return subDetails;
                        }

                        continue;
                    }

                    var details = From(task, ids);
                    if (details.Requires != null || details.Drops != null || details.CreatesOrAlters != null || details.Script != null)
                    {
                        yield return details;
                    }
                    else
                    {
                        ids.Remove(task); //release the id of this task back to the pool
                    }
                }
            }

            public static ScriptTaskDetails From(IScriptTask task, IDictionary<IScriptTask, string> ids)
            {
                if (!ids.ContainsKey(task))
                {
                    ids.Add(task, $"{ids.Count}");
                }

                return new ScriptTaskDetails
                {
                    Requires = task.Requires?.Any() == true ? task.Requires : null,
                    Drops = GetDependencies(task.ResultsIn, DependencyAction.Drop),
                    CreatesOrAlters = GetDependencies(task.ResultsIn, DependencyAction.CreateOrAlter),
                    Script = GetScript(task),
                    Id = ids[task]
                };
            }

            private static IReadOnlyCollection<ScriptTaskDependency> GetDependencies(
                IReadOnlyCollection<ScriptTaskDependency> dependencies,
                DependencyAction action)
            {
                if (dependencies == null)
                {
                    return null;
                }

                var matching = dependencies.Where(d => d.Action == action).ToArray();
                return matching.Any()
                    ? matching
                    : null;
            }

            private static string GetScript(IScriptTask task)
            {
                var output = new InterceptingOutput();
                task.WriteToOutput(output).Wait();

                return output.WrittenOutput;
            }
        }

        private class InterceptingOutput : IOutput
        {
            public string WrittenOutput { get; set; }

            public Task WriteJsonToOutput<T>(T output)
            {
                throw new NotSupportedException();
            }

            public Task WriteToOutput(string output)
            {
                WrittenOutput = output;
                return Task.CompletedTask;
            }
        }

        private class TableComponentNameConverter : JsonConverter<TableComponentName>
        {
            public override TableComponentName ReadJson(JsonReader reader, Type objectType, TableComponentName existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                throw new NotSupportedException();
            }

            public override void WriteJson(JsonWriter writer, TableComponentName value, JsonSerializer serializer)
            {
                writer.WriteValue(value.DebugString());
            }
        }
    }
}
