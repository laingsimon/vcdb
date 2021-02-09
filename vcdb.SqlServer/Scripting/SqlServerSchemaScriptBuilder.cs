using System.Collections.Generic;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;
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

        public IEnumerable<IOutputable> CreateUpgradeScripts(
            IReadOnlyCollection<SchemaDifference> schemaDifferences,
            IReadOnlyCollection<ISchemaObjectDifference> schemaObjectDifferences)
        {
            var transferScope = new SchemaObjectTransferScope(schemaObjectDifferences);

            foreach (var difference in schemaDifferences)
            {
                if (difference.SchemaAdded)
                {
                    yield return GetCreateSchemaScript(difference.RequiredSchema);

                    foreach (var script in transferScope.CreateTransferScriptsIntoCreatedSchema(difference.RequiredSchema.Key))
                    {
                        yield return script;
                    }

                    foreach (var script in permissionScriptBuilder.CreateSchemaPermissionScripts(
                        difference.RequiredSchema.Key,
                        PermissionDifferences.From(difference.RequiredSchema.Value.Permissions)))
                    {
                        yield return script;
                    }

                    continue;
                }

                if (difference.SchemaDeleted)
                {
                    foreach (var script in transferScope.CreateTransferScriptsAwayFromDroppedSchema(difference.CurrentSchema.Key))
                    {
                        yield return script;
                    }

                    yield return GetDropSchemaScript(difference.CurrentSchema);
                    continue;
                }

                if (difference.SchemaRenamedTo != null)
                {
                    yield return GetCreateSchemaScript(difference.RequiredSchema);

                    foreach (var script in transferScope.CreateTransferScriptsForRenamedSchema(difference.CurrentSchema.Key, difference.RequiredSchema.Key))
                    {
                        yield return script;
                    }

                    yield return GetDropSchemaScript(difference.CurrentSchema);
                }

                if (difference.DescriptionChangedTo != null)
                {
                    yield return descriptionScriptBuilder.ChangeSchemaDescription(
                        difference.RequiredSchema.Key, 
                        difference.CurrentSchema.Value.Description, 
                        difference.DescriptionChangedTo.Value);
                }

                foreach (var script in permissionScriptBuilder.CreateSchemaPermissionScripts(
                    difference.RequiredSchema.Key,
                    difference.PermissionDifferences))
                {
                    yield return script;
                }
            }

            foreach (var script in transferScope.CreateTransferScriptsForUnProcessedObjects())
            {
                yield return script;
            }
        }

        private SqlScript GetDropSchemaScript(NamedItem<string, SchemaDetails> currentSchema)
        {
            return new SqlScript($@"DROP SCHEMA [{currentSchema.Key}]
GO");
        }

        private SqlScript GetCreateSchemaScript(NamedItem<string, SchemaDetails> requiredSchema)
        {
            return new SqlScript($@"CREATE SCHEMA [{requiredSchema.Key}]
GO");
        }
    }
}
