using System.Collections.Generic;
using System.Linq;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;
using vcdb.Scripting.Permission;
using vcdb.Scripting.Schema;
using vcdb.Scripting.Table;

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

        public IEnumerable<SqlScript> CreateUpgradeScripts(IReadOnlyCollection<SchemaDifference> schemaDifferences, IReadOnlyCollection<TableDifference> tableDifferences)
        {
            var processedTableTransfers = new List<TableDifference>();

            foreach (var difference in schemaDifferences)
            {
                if (difference.SchemaAdded)
                {
                    yield return GetCreateSchemaScript(difference.RequiredSchema);

                    foreach (var transfer in CreateTransferTableScripts(null, difference.RequiredSchema.Key, tableDifferences))
                    {
                        processedTableTransfers.Add(transfer.TableDifference);
                        yield return transfer.SqlScript;
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
                    foreach (var transfer in CreateTransferTableScripts(difference.CurrentSchema.Key, null, tableDifferences))
                    {
                        processedTableTransfers.Add(transfer.TableDifference);
                        yield return transfer.SqlScript;
                    }

                    yield return GetDropSchemaScript(difference.CurrentSchema);
                    continue;
                }

                if (difference.SchemaRenamedTo != null)
                {
                    yield return GetCreateSchemaScript(difference.RequiredSchema);

                    foreach (var transfer in CreateTransferTableScripts(difference.CurrentSchema.Key, difference.RequiredSchema.Key, tableDifferences))
                    {
                        processedTableTransfers.Add(transfer.TableDifference);
                        yield return transfer.SqlScript;
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

            var unprocessedTransfers = tableDifferences.Except(processedTableTransfers).ToArray();
            foreach (var difference in unprocessedTransfers)
            {
                if (difference.TableAdded || difference.TableDeleted)
                    continue; //ignore added and removed tables

                //transfer tables...
                foreach (var transfer in CreateTransferTableScripts(difference.CurrentTable.Key.Schema, difference.RequiredTable.Key.Schema, unprocessedTransfers))
                {
                    yield return transfer.SqlScript;
                }
            }
        }

        private IEnumerable<TableTransfer> CreateTransferTableScripts(
            string currentSchemaName,
            string requiredSchemaName,
            IReadOnlyCollection<TableDifference> tableDifferences)
        {
            foreach (var tableDifference in tableDifferences.Where(diff => diff.TableRenamedTo != null))
            {
                var oldName = tableDifference.CurrentTable.Key;
                var newName = tableDifference.TableRenamedTo;

                if (oldName.Schema == newName.Schema)
                    continue;

                //there is a change of schema
                if (currentSchemaName != null && oldName.Schema == currentSchemaName)
                {
                    //schema is being removed or renamed
                    yield return new TableTransfer(new SqlScript(@$"ALTER SCHEMA {newName.Schema.SqlSafeName()}
TRANSFER {oldName.SqlSafeName()}
GO"), tableDifference);
                } else if (requiredSchemaName != null && newName.Schema == requiredSchemaName)
                {
                    //schema has been created, tables are supposed to be moved to it
                    yield return new TableTransfer(new SqlScript(@$"ALTER SCHEMA {newName.Schema.SqlSafeName()}
TRANSFER {oldName.SqlSafeName()}
GO"), tableDifference);
                }
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

        private class TableTransfer
        {
            public SqlScript SqlScript { get; }
            public TableDifference TableDifference { get; }

            public TableTransfer(SqlScript sqlScript, TableDifference tableDifference)
            {
                SqlScript = sqlScript;
                TableDifference = tableDifference;
            }
        }
    }
}
