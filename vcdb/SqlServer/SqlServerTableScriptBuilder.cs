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
DROP TABLE {table.SqlSafeName()}
GO");
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
                yield return new SqlScript($@"ALTER TABLE {requiredTableName.SqlSafeName()}
{string.Join(",", drops.Select(col => $"DROP COLUMN {col.CurrentColumn.SqlSafeName()}"))}
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

            foreach (var script in GetAlterIndexScripts(requiredTableName, tableDifference.IndexDifferences))
                yield return script;
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

                yield return new SqlScript($@"ALTER TABLE {tableName.SqlSafeName()}
ALTER COLUMN {columnName.SqlSafeName()} {column.Type}{nullabilityClause}
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
            return new SqlScript($@"ALTER TABLE {tableName.SqlSafeName()}
ALTER COLUMN {columnName.SqlSafeName()} DROP DEFAULT
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
            yield return new SqlScript($@"ALTER TABLE {tableName.SqlSafeName()}
ADD CONSTRAINT {(column.DefaultName ?? unnamedDefaultConstraint).SqlSafeName()}
DEFAULT ({column.Default})
FOR {columnName.SqlSafeName()}
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

            yield return new SqlScript($@"ALTER TABLE {tableName.SqlSafeName()}
ADD {columnName.SqlSafeName()} {column.Type}{nullabilityClause}
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
                yield return new SqlScript(@$"EXEC sp_rename 
    @objname = '{current.Schema}.{current.Table}', 
    @newname = '{required.Table}', 
    @objtype = 'OBJECT'
GO");
            }

            if (current.Schema != required.Schema)
            {
                var fromTableName = new TableName
                {
                    Schema = current.Schema,
                    Table = required.Table
                };

                yield return new SqlScript(@$"ALTER SCHEMA {required.Schema.SqlSafeName()}
TRANSFER {fromTableName.SqlSafeName()}
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
    @objname = '{currentTableName.Schema.SqlSafeName()}.{(currentConstraintName ?? currentAutomaticConstraintName).SqlSafeName()}', 
    @newname = '{(requiredConstraintName ?? requiredAutomaticConstraintName).SqlSafeName()}', 
    @objtype = 'OBJECT'
GO");
        }

        private SqlScript GetRenameIndexScript(TableName tableName, string currentName, string requiredName)
        {
            return new SqlScript(@$"EXEC sp_rename
    @objname = '{tableName.Schema}.{tableName.Table}.{currentName}',
    @newname = '{requiredName}',
    @objtype = 'INDEX'
GO");
        }

        private IEnumerable<SqlScript> GetCreateTableScript(TableDetails requiredTable, TableName tableName)
        {
            var columns = requiredTable.Columns.Select(CreateTableColumn);
            yield return new SqlScript($@"CREATE TABLE {tableName.SqlSafeName()} (
{string.Join("," + Environment.NewLine, columns)}
)
GO");

            foreach (var columnWithDefault in requiredTable.Columns
                .Where(col => col.Value.Default != null))
            {
                foreach (var script in GetAddDefaultScript(tableName, columnWithDefault.Key, columnWithDefault.Value))
                    yield return script;
            }

            foreach (var index in requiredTable.Indexes)
            {
                yield return GetAddIndexScript(tableName, index.Key, index.Value);
            }
        }

        private SqlScript GetAddIndexScript(TableName tableName, string indexName, IndexDetails index)
        {
            var uniqueClause = index.Unique
                ? "UNIQUE "
                : "";
            var clusteredClause = index.Clustered
                ? "CLUSTERED "
                : "";

            static string IndexColumnSpec(KeyValuePair<string, IndexColumnDetails> col)
            {
                var descendingClause = col.Value.Descending
                    ? " DESC"
                    : "";
                return $"{col.SqlSafeName()}{descendingClause}";
            }

            var columns = string.Join(", ", index.Columns.Select(IndexColumnSpec));
            var includeClause = index.Including?.Any() == true
                ? $"\r\nINCLUDE ({string.Join(", ", index.Including.Select(col => col.SqlSafeName()))})"
                : "";

            return new SqlScript($@"CREATE {uniqueClause}{clusteredClause}INDEX {indexName.SqlSafeName()}
ON {tableName.SqlSafeName()} ({columns}){includeClause}
GO");
        }

        private IEnumerable<SqlScript> GetAlterIndexScripts(TableName requiredTableName, IReadOnlyCollection<IndexDifference> indexDifferences)
        {
            foreach (var indexDifference in indexDifferences)
            {
                if (indexDifference.IndexAdded)
                {
                    yield return GetAddIndexScript(requiredTableName, indexDifference.RequiredIndex.Key, indexDifference.RequiredIndex.Value);
                    continue;
                }

                if (indexDifference.IndexDeleted)
                {
                    yield return GetDropIndexScript(requiredTableName, indexDifference.CurrentIndex.Key);
                    continue;
                }

                if (indexDifference.ClusteredChangedTo != null || indexDifference.ChangedColumns.Any() || indexDifference.ChangedIncludedColumns.Any() || indexDifference.UniqueChangedTo != null)
                {
                    //Indexes cannot be altered, they have to be dropped and re-created
                    yield return GetDropIndexScript(requiredTableName, indexDifference.CurrentIndex.Key);
                    yield return GetAddIndexScript(requiredTableName, indexDifference.RequiredIndex.Key, indexDifference.RequiredIndex.Value);
                } else if (indexDifference.IndexRenamedTo != null)
                {
                    yield return GetRenameIndexScript(requiredTableName, indexDifference.CurrentIndex.Key, indexDifference.IndexRenamedTo);
                }
            }
        }

        private SqlScript GetDropIndexScript(TableName requiredTableName, string indexName)
        {
            return new SqlScript($@"DROP INDEX {indexName.SqlSafeName()} ON {requiredTableName.SqlSafeName()}
GO");
        }

        private string CreateTableColumn(KeyValuePair<string, ColumnDetails> column)
        {
            var nullabilityClause = column.Value.Nullable == true
                ? ""
                : " NOT NULL";

            return $"  {column.SqlSafeName()} {column.Value.Type}{nullabilityClause}";
        }
    }
}
