using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;

namespace vcdb.SqlServer
{
    public class SqlServerTableScriptBuilder : ITableScriptBuilder
    {
        private readonly IColumnComparer columnComparer;

        public SqlServerTableScriptBuilder(IColumnComparer columnComparer)
        {
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
                {
                    foreach (var script in GetCreateTableScript(requiredTable.Value, requiredTable.Key))
                    {
                        yield return script;
                    }
                }
                else
                {
                    processedCurrentTables.Add(currentTable.Key);
                    if (!currentTable.Key.Equals(requiredTable.Key))
                    {
                        yield return await GetRenameTableScript(currentTable.Key, requiredTable.Key);

                        var columnsWithUnamedDefaults = currentTable.Value.Columns.Where(col => col.Value.Default != null && col.Value.DefaultName == null);
                        foreach (var columnWhereDefaultRequiresANameChange in columnsWithUnamedDefaults)
                        {
                            yield return GetRenameDefaultScript(
                                currentTable.Key,
                                requiredTable.Key,
                                columnWhereDefaultRequiresANameChange.AsNamedItem(),
                                null,
                                columnWhereDefaultRequiresANameChange.AsNamedItem(),
                                null);
                        }
                    }

                    var scripts = GetAlterTableScript(currentTable.Value, requiredTable.Value, requiredTable.Key);
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

            foreach (var rename in differentColumns.Where(difference => difference.ColumnRenamedTo != null))
            {
                yield return GetRenameColumnScript(tableName, rename.CurrentColumn.Key, rename.RequiredColumn.Key);

                if (rename.DefaultHasChanged && rename.RequiredColumn.Value.Default != null && rename.DefaultRenamedTo == null)
                {
                    //rename the default too
                    yield return GetRenameDefaultScript(
                        tableName,
                        tableName,
                        rename.CurrentColumn, 
                        rename.CurrentColumn.Value.DefaultName,
                        rename.RequiredColumn,
                        rename.RequiredColumn.Value.DefaultName);
                }
            }

            var drops = differentColumns.Where(diff => diff.ColumnDeleted).ToArray();
            if (drops.Any())
            {
                yield return new SqlScript($@"ALTER TABLE [{tableName.Schema}].[{tableName.Table}]
{string.Join(",", drops.Select(col => $"DROP COLUMN [{col.CurrentColumn.Key}]"))}
GO");
            }

            foreach (var add in differentColumns.Where(diff => diff.ColumnAdded))
            {
                foreach (var script in GetAddColumnScript(tableName, add.RequiredColumn.Key, add.RequiredColumn.Value))
                    yield return script;
            }

            var alterations = differentColumns.Where(IsAlteration);
            foreach (var alteration in alterations)
            {
                foreach (var script in GetAlterColumnScript(tableName, alteration))
                {
                    yield return script;
                }
            }
        }

        private bool IsAlteration(ColumnDifference difference)
        {
            if (difference.ColumnAdded || difference.ColumnDeleted)
                return false;

            return difference.DefaultHasChanged
                || difference.DefaultRenamedTo != null
                || difference.NullabilityChangedTo != null
                || difference.TypeChangedTo != null;
        }

        private IEnumerable<SqlScript> GetAlterColumnScript(TableName tableName, ColumnDifference alteration)
        {
            var column = alteration.RequiredColumn.Value;
            var columnName = alteration.RequiredColumn.Key;

            if (alteration.NullabilityChangedTo != null || alteration.TypeChangedTo != null)
            {
                //type of nullability has changed
                var nullabilityClause = column.Nullable == true
                    ? ""
                    : " NOT NULL";

                yield return new SqlScript($@"ALTER TABLE [{tableName.Schema}].[{tableName.Table}]
ALTER COLUMN [{columnName}] {column.Type}{nullabilityClause}
GO");
            }

            if (alteration.DefaultRenamedTo != null)
            {
                yield return GetRenameDefaultScript(
                    tableName, 
                    tableName,
                    alteration.CurrentColumn,
                    alteration.CurrentColumn.Value.DefaultName,
                    alteration.RequiredColumn,
                    alteration.DefaultRenamedTo);
            }

            if (alteration.DefaultHasChanged)
            {
                if (alteration.DefaultChangedTo == null)
                    yield return GetDropDefaultScript(tableName, columnName);
                else
                    yield return GetAlterDefaultScript(tableName, columnName, alteration.RequiredColumn.Value);
            }
        }

        private SqlScript GetRenameDefaultScript(
            TableName currentTableName,
            TableName requiredTableName,
            NamedItem<string, ColumnDetails> currentColumn, 
            string currentName,
            NamedItem<string, ColumnDetails> requiredColumn,
            string requiredName)
        {
            return new SqlScript(@$"EXEC sp_rename 
    @objname = '[{currentTableName.Schema}].[{currentName ?? GetNameForColumnDefault(currentTableName, currentColumn)}]', 
    @newname = '[{requiredName ?? GetNameForColumnDefault(requiredTableName, requiredColumn)}]', 
    @objtype = 'OBJECT'
GO");
        }

        private string GetNameForColumnDefault(TableName tableName, NamedItem<string, ColumnDetails> column)
        {
            return $"DF_{tableName.Table}_{column.Key}";
        }

        public SqlScript GetDropDefaultScript(TableName tableName, string columnName)
        {
            return new SqlScript($@"ALTER TABLE [{tableName.Schema}].[{tableName.Table}]
ALTER COLUMN [{columnName}] DROP DEFAULT
GO");
        }

        public SqlScript GetAlterDefaultScript(TableName tableName, string columnName, ColumnDetails column)
        {
            //TODO: What if the default has either a) changed value (so we can't add another default)

            throw new NotImplementedException();
        }

        public SqlScript GetAddDefaultScript(TableName tableName, string columnName, ColumnDetails column)
        {
            return new SqlScript($@"ALTER TABLE [{tableName.Schema}].[{tableName.Table}]
ADD CONSTRAINT [{column.DefaultName ?? GetNameForColumnDefault(tableName, new NamedItem<string, ColumnDetails>(columnName, column))}]
DEFAULT ({column.Default})
FOR [{columnName}]
GO");
        }

        private IEnumerable<SqlScript> GetAddColumnScript(TableName tableName, string columnName, ColumnDetails column)
        {
            var nullabilityClause = column.Nullable == true
                ? ""
                : " NOT NULL";

            yield return new SqlScript($@"ALTER TABLE [{tableName.Schema}].[{tableName.Table}]
ADD [{columnName}] {column.Type}{nullabilityClause}
GO");

            if (column.Default != null)
                yield return GetAddDefaultScript(tableName, columnName, column);
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

        private IEnumerable<SqlScript> GetCreateTableScript(TableDetails requiredTable, TableName tableName)
        {
            var columns = requiredTable.Columns.Select(CreateTableColumn);
            yield return new SqlScript($@"
CREATE TABLE [{tableName.Schema}].[{tableName.Table}] (
{string.Join("," + Environment.NewLine, columns)}
)
GO");

            foreach (var columnWithDefault in requiredTable.Columns
                .Where(col => col.Value.Default != null))
            {
                yield return GetAddDefaultScript(tableName, columnWithDefault.Key, columnWithDefault.Value);
            }

            //TODO: Add indexes
        }

        private string CreateTableColumn(KeyValuePair<string, ColumnDetails> column)
        {
            var nullabilityClause = column.Value.Nullable == true
                ? ""
                : " NOT NULL";

            return $"  [{column.Key}] {column.Value.Type}{nullabilityClause}";
        }

        private NamedItem<TableName, TableDetails> GetCurrentTable(
            IDictionary<TableName, TableDetails> currentTables,
            KeyValuePair<TableName, TableDetails> requiredTable)
        {
            return GetCurrentTable(currentTables, requiredTable.Key)
                ?? GetCurrentTableForPreviousName(currentTables, requiredTable.Value.PreviousNames);
        }

        private NamedItem<TableName, TableDetails> GetCurrentTable(
            IDictionary<TableName, TableDetails> currentTables,
            TableName requiredTableName)
        {
            var tablesWithSameName = currentTables.Where(pair => pair.Key.Equals(requiredTableName)).ToArray();
            return tablesWithSameName.Length == 1
                ? tablesWithSameName[0].AsNamedItem()
                : NamedItem<TableName, TableDetails>.Null;
        }

        private NamedItem<TableName, TableDetails> GetCurrentTableForPreviousName(
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
