using System.Collections.Generic;
using System.Linq;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;
using vcdb.Scripting.ExecutionPlan;
using vcdb.Scripting.Permission;
using vcdb.Scripting.Schema;

namespace vcdb.SqlServer.Scripting
{
    public class SqlServerSchemaScriptBuilder : ISchemaScriptBuilder
    {
        private readonly IDescriptionScriptBuilder descriptionScriptBuilder;
        private readonly IPermissionScriptBuilder permissionScriptBuilder;

        public SqlServerSchemaScriptBuilder(
            IDescriptionScriptBuilder descriptionScriptBuilder,
            IPermissionScriptBuilder permissionScriptBuilder)
        {
            this.descriptionScriptBuilder = descriptionScriptBuilder;
            this.permissionScriptBuilder = permissionScriptBuilder;
        }

        public IEnumerable<IScriptTask> CreateUpgradeScripts(
            IReadOnlyCollection<SchemaDifference> schemaDifferences,
            IReadOnlyCollection<ISchemaObjectDifference> schemaObjectDifferences)
        {
            var transferScope = new SchemaObjectTransferScope(schemaObjectDifferences);

            foreach (var difference in schemaDifferences)
            {
                if (difference.SchemaAdded)
                {
                    yield return new MultiScriptTask(
                        new[] { GetCreateSchemaScript(difference.RequiredSchema) }
                        .Concat(transferScope.CreateTransferScriptsIntoCreatedSchema(difference.RequiredSchema.Key))
                        .Concat(permissionScriptBuilder.CreateSchemaPermissionScripts(
                            difference.RequiredSchema.Key,
                            PermissionDifferences.From(difference.RequiredSchema.Value.Permissions)))
                    );

                    continue;
                }

                if (difference.SchemaDeleted)
                {
                    yield return new MultiScriptTask(
                        transferScope.CreateTransferScriptsAwayFromDroppedSchema(difference.CurrentSchema.Key)
                        .Concat(new[] { GetDropSchemaScript(difference.CurrentSchema) }));
                    continue;
                }

                if (difference.SchemaRenamedTo != null)
                {
                    yield return new MultiScriptTask(
                        new[] { GetCreateSchemaScript(difference.RequiredSchema) }
                        .Concat(transferScope.CreateTransferScriptsForRenamedSchema(difference.CurrentSchema.Key, difference.RequiredSchema.Key))
                        .Concat(new[] { GetDropSchemaScript(difference.CurrentSchema) }));
                }

                if (difference.DescriptionChangedTo != null)
                {
                    yield return descriptionScriptBuilder.ChangeSchemaDescription(
                        difference.RequiredSchema.Key, 
                        difference.CurrentSchema.Value.Description, 
                        difference.DescriptionChangedTo.Value);
                }

                yield return new MultiScriptTask(permissionScriptBuilder.CreateSchemaPermissionScripts(
                    difference.RequiredSchema.Key,
                    difference.PermissionDifferences));
            }

            yield return new MultiScriptTask(transferScope.CreateTransferScriptsForUnProcessedObjects());
        }

        private IScriptTask GetDropSchemaScript(NamedItem<string, SchemaDetails> currentSchema)
        {
            return new SqlScript($@"DROP SCHEMA [{currentSchema.Key}]
GO").Drops().Schema(currentSchema.Key);
        }

        private IScriptTask GetCreateSchemaScript(NamedItem<string, SchemaDetails> requiredSchema)
        {
            return new SqlScript($@"CREATE SCHEMA [{requiredSchema.Key}]
GO").CreatesOrAlters().Schema(requiredSchema.Key);
        }
    }
}
