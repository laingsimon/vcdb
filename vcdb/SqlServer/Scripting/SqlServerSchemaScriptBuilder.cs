using System.Collections.Generic;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;

namespace vcdb.SqlServer.Scripting
{
    public class SqlServerSchemaScriptBuilder : ISchemaScriptBuilder
    {
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
