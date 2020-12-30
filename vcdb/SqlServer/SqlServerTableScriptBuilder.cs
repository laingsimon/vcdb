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
            IDictionary<TableName, TableDetails> current, 
            IDictionary<TableName, TableDetails> required)
        {
            foreach (var requiredTable in required)
            {
                var currentTable = GetCurrentTable(current, requiredTable);
                if (currentTable == null)
                    yield return await GetCreateTableScript(requiredTable.Value, requiredTable.Key);
                else
                {
                    var currentTableInst = currentTable.Value;
                    if (currentTableInst.Key != requiredTable.Key)
                        yield return await GetRenameTableScript(currentTableInst.Key, requiredTable.Key);

                    yield return await GetAlterTableScript(currentTableInst.Value, requiredTable.Value, requiredTable.Key);
                }
            }
        }

        private async Task<SqlScript> GetAlterTableScript(TableDetails currentTable, TableDetails requiredTable, TableName tableName)
        {
            throw new System.NotImplementedException();
        }

        private Task<SqlScript> GetRenameTableScript(TableName current, TableName required)
        {
            return Task.FromResult(new SqlScript(@$"
exec sp_rename 
    @old_name = '{current.Schema}.{current.Table}', 
    @new_name = '{required.Schema}.{required.Table}', 
    @object_type = 'table';"));
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
            IDictionary<TableName, TableDetails> current,
            KeyValuePair<TableName, TableDetails> requiredTable)
        {
            var tablesWithSameName = current.Where(pair => pair.Key == requiredTable.Key).ToArray();
            if (tablesWithSameName.Length == 1)
            {
                return tablesWithSameName[0];
            }

            //TODO: look for tables with the name set to requiredTable.Value.PreviousNames
            return null;
        }
    }
}
