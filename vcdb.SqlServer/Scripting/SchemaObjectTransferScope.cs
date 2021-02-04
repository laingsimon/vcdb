using System.Collections.Generic;
using System.Linq;
using vcdb.Output;
using vcdb.Scripting.Schema;

namespace vcdb.SqlServer.Scripting
{
    public class SchemaObjectTransferScope
    {
        private readonly IReadOnlyCollection<ISchemaObjectDifference> differences;
        private readonly List<ISchemaObjectDifference> processedDifferences = new List<ISchemaObjectDifference>();

        public SchemaObjectTransferScope(IReadOnlyCollection<ISchemaObjectDifference> differences)
        {
            this.differences = differences;
        }

        public IEnumerable<SqlScript> CreateTransferScriptsIntoCreatedSchema(string createdSchemaName)
        {
            foreach (var transfer in CreateTransferScripts(null, createdSchemaName, differences))
            {
                processedDifferences.Add(transfer.Difference);
                yield return transfer.SqlScript;
            }
        }

        public IEnumerable<SqlScript> CreateTransferScriptsAwayFromDroppedSchema(string droppedSchemaName)
        {
            foreach (var transfer in CreateTransferScripts(droppedSchemaName, null, differences))
            {
                processedDifferences.Add(transfer.Difference);
                yield return transfer.SqlScript;
            }
        }

        public IEnumerable<SqlScript> CreateTransferScriptsForRenamedSchema(string currentName, string requiredName)
        {
            foreach (var transfer in CreateTransferScripts(currentName, requiredName, differences))
            {
                processedDifferences.Add(transfer.Difference);
                yield return transfer.SqlScript;
            }
        }

        public IEnumerable<SqlScript> CreateTransferScriptsForUnProcessedObjects()
        {
            var unprocessedDifferences = differences.Except(processedDifferences).ToArray();
            foreach (var difference in unprocessedDifferences)
            {
                if (difference.ObjectAdded || difference.ObjectDeleted)
                    continue; //ignore added and removed objects

                //transfer objects...
                foreach (var transfer in CreateTransferScripts(difference.CurrentName.Schema, difference.RequiredName.Schema, unprocessedDifferences))
                {
                    yield return transfer.SqlScript;
                }
            }
        }

        private IEnumerable<SchemaConstiuentTransfer> CreateTransferScripts(
            string currentSchemaName,
            string requiredSchemaName,
            IReadOnlyCollection<ISchemaObjectDifference> differences)
        {
            foreach (var difference in differences.Where(diff => diff.ObjectRenamedTo != null))
            {
                var oldName = difference.CurrentName;
                var newName = difference.ObjectRenamedTo;

                if (oldName.Schema == newName.Schema)
                    continue;

                //there is a change of schema
                if (currentSchemaName != null && oldName.Schema == currentSchemaName)
                {
                    //schema is being removed or renamed
                    yield return new SchemaConstiuentTransfer(new SqlScript(@$"ALTER SCHEMA {newName.Schema.SqlSafeName()}
TRANSFER {oldName.SqlSafeName()}
GO"), difference);
                }
                else if (requiredSchemaName != null && newName.Schema == requiredSchemaName)
                {
                    //schema has been created, tables are supposed to be moved to it
                    yield return new SchemaConstiuentTransfer(new SqlScript(@$"ALTER SCHEMA {newName.Schema.SqlSafeName()}
TRANSFER {oldName.SqlSafeName()}
GO"), difference);
                }
            }
        }

        private class SchemaConstiuentTransfer
        {
            public SqlScript SqlScript { get; }
            public ISchemaObjectDifference Difference { get; }

            public SchemaConstiuentTransfer(SqlScript sqlScript, ISchemaObjectDifference difference)
            {
                SqlScript = sqlScript;
                Difference = difference;
            }
        }
    }
}
