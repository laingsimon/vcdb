using System;
using System.Collections.Generic;
using System.Linq;
using vcdb.CommandLine;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;

namespace vcdb.SqlServer
{
    public class SqlServerTableScriptBuilder : ITableScriptBuilder
    {
        private readonly Options options;
        private readonly ISqlObjectNameHelper objectNameHelper;

        public SqlServerTableScriptBuilder(Options options, ISqlObjectNameHelper objectNameHelper)
        {
            this.options = options;
            this.objectNameHelper = objectNameHelper;
        }

        public IEnumerable<SqlScript> CreateUpgradeScripts(IReadOnlyCollection<TableDifference> tableDifferences)
        {
            foreach (var tableDifference in tableDifferences)
            {
                var requiredTable = tableDifference.RequiredTable;
                var currentTable = tableDifference.CurrentTable;

                if (tableDifference.TableAdded)
                {
                    foreach (var script in GetCreateTableScript(requiredTable.Value, requiredTable.Key))
                    {
                        yield return script;
                    }
                    continue;
                }

                if (tableDifference.TableDeleted)
                {
                    yield return GetDropTableScript(currentTable.Key);
                    continue;
                }

                if (tableDifference.TableRenamedTo != null)
                {
                    foreach (var script in GetRenameTableScript(currentTable.Key, requiredTable.Key))
                        yield return script;

                    if (!options.IgnoreUnnamedDefaults)
                    {
                        var defaultConstraintRenames = GetRenameUnnamedDefaultsIfNoColumnsAreRenamed(tableDifference);
                        foreach (var defaultConstraintRename in defaultConstraintRenames)
                            yield return defaultConstraintRename;
                    }
                }

                foreach (var script in GetAlterTableScript(currentTable.Key, requiredTable.Key, tableDifference))
                {
                    yield return script;
                }
            }
        }

        private IEnumerable<SqlScript> GetRenameUnnamedDefaultsIfNoColumnsAreRenamed(TableDifference tableDifference)
        {
            var requiredTable = tableDifference.RequiredTable;
            var columnsWithDefaultsButNoExplicitName = requiredTable.Value.Columns.Where(col =>
            {
                return col.Value.Default != null && col.Value.DefaultName == null;
            });
            
            foreach (var requiredColumnWithUnnamedDefault in columnsWithDefaultsButNoExplicitName)
            {
                var columnDifference = tableDifference.ColumnDifferences.SingleOrDefault(cd => cd.RequiredColumn.Key == requiredColumnWithUnnamedDefault.Key);

                if (columnDifference != null)
                {
                    continue;
                }

                var currentColumn = new NamedItem<string, ColumnDetails>(
                    requiredColumnWithUnnamedDefault.Key,
                    tableDifference.CurrentTable.Value.Columns[requiredColumnWithUnnamedDefault.Key]);

                //The column hasn't changed (name) so the required and current column details are the same
                yield return GetRenameDefaultScript(
                    tableDifference.CurrentTable.Key,
                    tableDifference.RequiredTable.Key,
                    currentColumn,
                    currentColumn.Value.DefaultName,
                    currentColumn,
                    null);
            }
        }

        private SqlScript GetDropTableScript(TableName table)
        {
            return new SqlScript(@$"
DROP TABLE [{table.Schema}].[{table.Table}]");
        }

        private IEnumerable<SqlScript> GetAlterTableScript(TableName currentTableName, TableName requiredTableName, TableDifference tableDifference)
        {
            var differentColumns = tableDifference.ColumnDifferences;
            foreach (var rename in differentColumns.Where(difference => difference.ColumnRenamedTo != null))
            {
                var requiredColumn = rename.RequiredColumn;
                var currentColumn = rename.CurrentColumn;
                yield return GetRenameColumnScript(requiredTableName, currentColumn.Key, requiredColumn.Key);

                if (!rename.DefaultHasChanged && requiredColumn.Value.Default != null && rename.DefaultRenamedTo == null && currentColumn.Value.DefaultName == null)
                {
                    if (!options.IgnoreUnnamedDefaults)
                    {
                        //rename the default too
                        yield return GetRenameDefaultScript(
                            currentTableName,
                            requiredTableName,
                            currentColumn,
                            currentColumn.Value.DefaultName,
                            requiredColumn,
                            requiredColumn.Value.DefaultName);
                    }
                }
            }

            var drops = differentColumns.Where(diff => diff.ColumnDeleted).ToArray();
            if (drops.Any())
            {
                yield return new SqlScript($@"ALTER TABLE [{requiredTableName.Schema}].[{requiredTableName.Table}]
{string.Join(",", drops.Select(col => $"DROP COLUMN [{col.CurrentColumn.Key}]"))}
GO");
            }

            foreach (var add in differentColumns.Where(diff => diff.ColumnAdded))
            {
                foreach (var script in GetAddColumnScript(requiredTableName, add.RequiredColumn.Key, add.RequiredColumn.Value))
                    yield return script;
            }

            var alterations = differentColumns.Where(IsAlteration);
            foreach (var alteration in alterations)
            {
                foreach (var script in GetAlterColumnScript(requiredTableName, alteration))
                {
                    yield return script;
                }
            }

            //TODO: Yield index changes
        }

        private bool IsAlteration(ColumnDifference difference)
        {
            return !difference.ColumnAdded && !difference.ColumnDeleted && difference.IsChanged;
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
                {
                    foreach (var script in GetAlterDefaultScript(tableName, columnName, alteration.RequiredColumn.Value))
                        yield return script;
                }
            }
        }

        private string GetNameForColumnDefault(TableName tableName, NamedItem<string, ColumnDetails> column)
        {
            return objectNameHelper.GetAutomaticConstraintName("DF", tableName.Table, column.Key, column.Value.DefaultObjectId ?? 0);
        }

        public SqlScript GetDropDefaultScript(TableName tableName, string columnName)
        {
            return new SqlScript($@"ALTER TABLE [{tableName.Schema}].[{tableName.Table}]
ALTER COLUMN [{columnName}] DROP DEFAULT
GO");
        }

        public IEnumerable<SqlScript> GetAlterDefaultScript(TableName tableName, string columnName, ColumnDetails column)
        {
            yield return GetDropDefaultScript(tableName, columnName);
            foreach (var script in GetAddDefaultScript(tableName, columnName, column))
                yield return script;
        }

        public IEnumerable<SqlScript> GetAddDefaultScript(TableName tableName, string columnName, ColumnDetails column)
        {
            var unnamedDefaultConstraint = GetNameForColumnDefault(tableName, new NamedItem<string, ColumnDetails>(columnName, column));
            yield return new SqlScript($@"ALTER TABLE [{tableName.Schema}].[{tableName.Table}]
ADD CONSTRAINT [{column.DefaultName ?? unnamedDefaultConstraint}]
DEFAULT ({column.Default})
FOR [{columnName}]
GO");

            if (column.DefaultName == null)
            {
                yield return new SqlScript(@$"DECLARE @newName VARCHAR(1024)
SELECT @newName = 'DF__{tableName.Table}__{columnName}__' + FORMAT(def.OBJECT_ID, 'X')
FROM sys.default_constraints def
INNER JOIN sys.columns col
ON col.column_id = def.parent_column_id
AND col.object_id = def.parent_object_id
INNER JOIN sys.tables tab
ON tab.object_id = col.object_id
WHERE tab.name = '{tableName.Table}'
AND SCHEMA_NAME(tab.schema_id) = '{tableName.Schema}'
AND def.name = '{unnamedDefaultConstraint}'

EXEC sp_rename 
    @objname = '{unnamedDefaultConstraint}', 
    @newname = @newName, 
    @objtype = 'OBJECT'
GO");
            }
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
            {
                foreach (var script in GetAddDefaultScript(tableName, columnName, column))
                    yield return script;
            }
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

        private IEnumerable<SqlScript> GetRenameTableScript(TableName current, TableName required)
        {
            if (current.Table != required.Table)
            {
                yield return new SqlScript(@$"
EXEC sp_rename 
    @objname = '{current.Schema}.{current.Table}', 
    @newname = '{required.Table}', 
    @objtype = 'OBJECT'
GO");
            }

            if (current.Schema != required.Schema)
            {
                yield return new SqlScript(@$"ALTER SCHEMA [{required.Schema}]
TRANSFER [{current.Schema}].[{required.Table}]
GO");
            }
        }

        private SqlScript GetRenameDefaultScript(
            TableName currentTableName,
            TableName requiredTableName,
            NamedItem<string, ColumnDetails> currentColumn,
            string currentConstraintName,
            NamedItem<string, ColumnDetails> requiredColumn,
            string requiredConstraintName)
        {
            var currentAutomaticConstraintName = objectNameHelper.GetAutomaticConstraintName(
                "DF",
                currentTableName.Table,
                currentColumn.Key,
                currentColumn.Value.DefaultObjectId ?? 0);
            var requiredAutomaticConstraintName = objectNameHelper.GetAutomaticConstraintName(
                "DF",
                requiredTableName.Table,
                requiredColumn.Key,
                currentColumn.Value.DefaultObjectId ?? 0);

            return new SqlScript(@$"EXEC sp_rename 
    @objname = '[{currentTableName.Schema}].[{currentConstraintName ?? currentAutomaticConstraintName}]', 
    @newname = '[{requiredConstraintName ?? requiredAutomaticConstraintName}]', 
    @objtype = 'OBJECT'
GO");
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
                foreach (var script in GetAddDefaultScript(tableName, columnWithDefault.Key, columnWithDefault.Value))
                    yield return script;
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
    }
}
