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
        private readonly IColumnComparer columnComparer;

        public SqlServerTableScriptBuilder(Options options, IColumnComparer columnComparer)
        {
            this.options = options;
            this.columnComparer = columnComparer;
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

                    var scripts = GetAlterTableScript(currentTableInst.Value, requiredTable.Value, requiredTable.Key);
                    foreach (var script in scripts)
                        yield return script;
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

        private IEnumerable<SqlScript> GetAlterTableScript(TableDetails currentTable, TableDetails requiredTable, TableName tableName)
        {
            var differentColumns = columnComparer.GetDifferentColumns(currentTable.Columns, requiredTable.Columns).ToArray();
            if (!differentColumns.Any())
                yield break;

            var renames = differentColumns.Where(IsRename);
            foreach (var rename in renames)
                yield return GetRenameColumnScript(tableName, rename.CurrentColumn.Value.Key, rename.RequiredColumn.Value.Key);
            var drops = differentColumns.Where(diff => diff.CurrentColumn != null && diff.RequiredColumn == null).ToArray();
            if (drops.Any())
                yield return new SqlScript($@"ALTER TABLE [{tableName.Schema}].[{tableName.Table}]
{string.Join(",", drops.Select(col => $"DROP COLUMN [{col.CurrentColumn.Value.Key}]"))}
GO");

            var adds = differentColumns.Where(diff => diff.CurrentColumn == null && diff.RequiredColumn != null);
            foreach (var add in adds)
                yield return GetAddColumnScript(tableName, add.RequiredColumn.Value.Key, add.RequiredColumn.Value.Value);

            var alterations = differentColumns.Where(IsAlteration);
            foreach (var alteration in alterations)
            {
                yield return GetAlterColumnScript(tableName, alteration.RequiredColumn.Value.Key, alteration.RequiredColumn.Value.Value);
            }
        }

        private bool IsRename(ColumnDifference difference)
        {
            return difference.CurrentColumn != null
                && difference.RequiredColumn != null
                && difference.CurrentColumn.Value.Key != difference.RequiredColumn.Value.Key;
        }

        private bool IsAlteration(ColumnDifference difference)
        {
            if (difference.CurrentColumn == null
                || difference.RequiredColumn == null)
                return false;

            var currentColumn = difference.CurrentColumn.Value.Value;
            var requiredColumn = difference.RequiredColumn.Value.Value;

            return currentColumn.Nullable != requiredColumn.Nullable
                || currentColumn.Type != requiredColumn.Type
                || IsDefaultDifferent(currentColumn.Default, requiredColumn.Default);
        }

        private bool IsDefaultDifferent(object currentDefault, object requiredDefault)
        {
            if (currentDefault == null && requiredDefault == null)
                return false;

            if (currentDefault != null && requiredDefault != null)
                return !currentDefault.Equals(requiredDefault);

            return true;
        }

        private SqlScript GetAlterColumnScript(TableName tableName, string columnName, ColumnDetails column)
        {
            var nullabilityClause = column.Nullable == true
                ? ""
                : " NOT NULL";
            var defaultClause = column.Default != null
                ? $" DEFAULT({column.Default})"
                : "";

            return new SqlScript($@"ALTER TABLE [{tableName.Schema}].[{tableName.Table}]
ALTER COLUMN [{columnName}] {column.Type}{nullabilityClause}{defaultClause}
GO");

        }

        private SqlScript GetAddColumnScript(TableName tableName, string columnName, ColumnDetails column)
        {
            var nullabilityClause = column.Nullable == true
                ? ""
                : " NOT NULL";
            var defaultClause = column.Default != null
                ? $" DEFAULT({column.Default})"
                : "";

            return new SqlScript($@"ALTER TABLE [{tableName.Schema}].[{tableName.Table}]
ADD [{columnName}] {column.Type}{nullabilityClause}{defaultClause}
GO");
        }

        private SqlScript GetRenameColumnScript(
            TableName tableName, 
            string currentColumnName, 
            string requiredColumnName)
        {
            return new SqlScript(@$"EXEC sp_rename
    @objname = '{tableName.Schema}.{tableName.Table}.{currentColumnName}',
    @newname = '{requiredColumnName}',
    @objtype = 'COLUMN'
GO");
        }

        private Task<SqlScript> GetRenameTableScript(TableName current, TableName required)
        {
            return Task.FromResult(new SqlScript(@$"
EXEC sp_rename 
    @objname = '{current.Schema}.{current.Table}', 
    @newname = '{required.Schema}.{required.Table}', 
    @objtype = 'OBJECT'
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
