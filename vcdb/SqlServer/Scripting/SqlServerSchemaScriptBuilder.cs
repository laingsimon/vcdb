using System;
using System.Collections.Generic;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;

namespace vcdb.SqlServer.Scripting
{
    public class SqlServerSchemaScriptBuilder : ISqlServerSchemaScriptBuilder, ISchemaScriptBuilder
    {
        private readonly IDescriptionScriptBuilder descriptionScriptBuilder;

        public SqlServerSchemaScriptBuilder(IDescriptionScriptBuilder descriptionScriptBuilder)
        {
            this.descriptionScriptBuilder = descriptionScriptBuilder;
        }

        public IEnumerable<SqlScript> CreateUpgradeScripts(IReadOnlyCollection<SchemaDifference> schemaDifferences)
        {
            throw new NotSupportedException(@"This script builder needs to know whether it is operating before or after table renames.
The table renames are responsible for moving tables between schemas too.

TODO: Move the table schema moves to be part of this script builder so there is no before/after requirement");
        }

        public IEnumerable<SqlScript> CreateUpgradeScripts(IReadOnlyCollection<SchemaDifference> schemaDifferences, bool beforeTableRenamesAndTransfers)
        {
            foreach (var difference in schemaDifferences)
            {
                if (difference.SchemaAdded && beforeTableRenamesAndTransfers)
                {
                    yield return GetCreateSchemaScript(difference.RequiredSchema);
                    continue;
                }

                if (difference.SchemaDeleted && !beforeTableRenamesAndTransfers)
                {
                    yield return GetDropSchemaScript(difference.CurrentSchema);
                    continue;
                }

                if (difference.SchemaRenamedTo != null)
                {
                    yield return GetRenameSchemaScript(difference, beforeTableRenamesAndTransfers);
                }

                if (difference.DescriptionHasChanged && !beforeTableRenamesAndTransfers)
                {
                    yield return descriptionScriptBuilder.ChangeSchemaDescription(
                        difference.RequiredSchema.Key, 
                        difference.CurrentSchema.Value.Description, 
                        difference.DescriptionChangedTo);
                }
            }
        }

        private SqlScript GetRenameSchemaScript(SchemaDifference difference, bool beforeTableRenamesAndTransfers)
        {
            if (beforeTableRenamesAndTransfers)
            {
                return GetCreateSchemaScript(difference.RequiredSchema);
            }

            return GetDropSchemaScript(difference.CurrentSchema);
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
