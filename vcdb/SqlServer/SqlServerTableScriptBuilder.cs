using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using vcdb.CommandLine;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;

namespace vcdb.SqlServer
{
    public class SqlServerTableScriptBuilder : ITableScriptBuilder
    {
        private readonly Options options;

        public SqlServerTableScriptBuilder(Options options)
        {
            this.options = options;
        }

        public async IAsyncEnumerable<SqlScript> CreateUpgradeScripts(
            IDictionary<TableName, TableDetails> currentTables, 
            IDictionary<TableName, TableDetails> requiredTables)
        {
            var processedCurrentTables = new HashSet<TableName>();
            foreach (var requiredTable in requiredTables)
            {
                var currentTable = GetCurrentTable(currentTables, requiredTable);
                if (currentTable == null)
                    yield return await GetCreateTableScript(requiredTable.Value, requiredTable.Key);
                else
                {
                    var currentTableInst = currentTable.Value;
                    processedCurrentTables.Add(currentTableInst.Key);
                    if (!currentTableInst.Key.Equals(requiredTable.Key))
                        yield return await GetRenameTableScript(currentTableInst.Key, requiredTable.Key);

                    yield return await GetAlterTableScript(currentTableInst.Value, requiredTable.Value, requiredTable.Key);
                }
            }

            foreach (var currentTable in currentTables.Where(t => !processedCurrentTables.Contains(t.Key)))
            {
                yield return await GetDropTableScript(currentTable.Key);
            }
        }

        private Task<SqlScript> GetDropTableScript(TableName table)
        {
            return Task.FromResult(new SqlScript(@$"
DROP TABLE [{table.Schema}].[{table.Table}]"));
        }

        private Task<SqlScript> GetAlterTableScript(TableDetails currentTable, TableDetails requiredTable, TableName tableName)
        {
            return Task.FromResult((SqlScript)null);
        }

        private Task<SqlScript> GetRenameTableScript(TableName current, TableName required)
        {
            return Task.FromResult(new SqlScript(@$"
EXEC sp_rename 
    @old_name = '{current.Schema}.{current.Table}', 
    @new_name = '{required.Schema}.{required.Table}', 
    @object_type = 'TABLE'
GO"));
        }

        private Task<SqlScript> GetCreateTableScript(TableDetails requiredTable, TableName tableName)
        {
            var columns = requiredTable.Columns.Select(CreateTableColumn);

            return Task.FromResult(new SqlScript($@"
CREATE TABLE [{tableName.Schema}].[{tableName.Table}] (
{string.Join("," + Environment.NewLine, columns)}
)"));
            //TODO: Add indexes
        }

        private string CreateTableColumn(KeyValuePair<string, ColumnDetails> column)
        {
            var nullabilityClause = column.Value.Nullable == true
                ? ""
                : " not null";

            var defaultClause = column.Value.Default != null
                ? $" default({column.Value.Default})"
                : "";

            return $"  [{column.Key}] {column.Value.Type}{nullabilityClause}{defaultClause}";
        }

        private KeyValuePair<TableName, TableDetails>? GetCurrentTable(
            IDictionary<TableName, TableDetails> currentTables,
            KeyValuePair<TableName, TableDetails> requiredTable)
        {
            return GetCurrentTable(currentTables, requiredTable.Key)
                ?? GetCurrentTableForPreviousName(currentTables, requiredTable.Value.PreviousNames);
        }

        private KeyValuePair<TableName, TableDetails>? GetCurrentTable(
            IDictionary<TableName, TableDetails> currentTables,
            TableName requiredTableName)
        {
            var tablesWithSameName = currentTables.Where(pair => pair.Key.Equals(requiredTableName)).ToArray();
            return tablesWithSameName.Length == 1
                ? tablesWithSameName[0]
                : default(KeyValuePair<TableName, TableDetails>?);
        }

        private KeyValuePair<TableName, TableDetails>? GetCurrentTableForPreviousName(
            IDictionary<TableName, TableDetails> currentTables,
            TableName[] previousNames)
        {
            if (previousNames == null)
                return null;

            return previousNames
                .Select(previousName => GetCurrentTable(currentTables, previousName))
                .FirstOrDefault(currentTable => currentTable != null);
        }
    }
}
