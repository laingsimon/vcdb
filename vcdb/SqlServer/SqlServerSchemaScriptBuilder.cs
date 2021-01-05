using System.Collections.Generic;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;

namespace vcdb.SqlServer
{
    public class SqlServerSchemaScriptBuilder : ISchemaScriptBuilder
    {
        public IEnumerable<SqlScript> CreateUpgradeScripts(IReadOnlyCollection<SchemaDifference> schemaDifferences)
        {
            foreach (var difference in schemaDifferences)
            {
                if (difference.SchemaAdded)
                {
                    yield return GetCreateSchemaScript(difference.RequiredSchema);
                    continue;
                }

                if (difference.SchemaDeleted)
                {
                    yield return GetDropSchemaScript(difference.CurrentSchema);
                    continue;
                }

                if (difference.SchemaRenamedTo != null)
                {
                    yield return GetRenameSchemaScript(difference);
                }
            }
        }

        private SqlScript GetRenameSchemaScript(SchemaDifference difference)
        {
            return new SqlScript($@"??RENAME SCHEMA??
GO");
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
