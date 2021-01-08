using System;
using System.Collections.Generic;
using System.Linq;
using vcdb.CommandLine;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;

namespace vcdb.SqlServer.Scripting
{
    public class SqlServerTableScriptBuilder : ITableScriptBuilder
    {
        private readonly IDescriptionScriptBuilder descriptionScriptBuilder;
        private readonly IIndexScriptBuilder indexScriptBuilder;
        private readonly IDefaultConstraintScriptBuilder defaultConstraintScriptBuilder;
        private readonly ICheckConstraintScriptBuilder checkConstraintScriptBuilder;

        public SqlServerTableScriptBuilder(
            IDescriptionScriptBuilder descriptionScriptBuilder,
            IIndexScriptBuilder indexScriptBuilder,
            IDefaultConstraintScriptBuilder defaultConstraintScriptBuilder,
            ICheckConstraintScriptBuilder checkConstraintScriptBuilder)
        {
            this.descriptionScriptBuilder = descriptionScriptBuilder;
            this.indexScriptBuilder = indexScriptBuilder;
            this.defaultConstraintScriptBuilder = defaultConstraintScriptBuilder;
            this.checkConstraintScriptBuilder = checkConstraintScriptBuilder;
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

                    foreach (var script in indexScriptBuilder.CreateUpgradeScripts(requiredTable.Key, tableDifference.IndexDifferences ?? new IndexDifference[0]))
                    {
                        yield return script;
                    }

                    foreach (var script in defaultConstraintScriptBuilder.CreateUpgradeScripts(tableDifference))
                    {
                        yield return script;
                    }

                    foreach (var script in checkConstraintScriptBuilder.CreateUpgradeScripts(tableDifference))
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
                }

                foreach (var script in GetAlterTableScript(tableDifference))
                {
                    yield return script;
                }

                foreach (var script in indexScriptBuilder.CreateUpgradeScripts(requiredTable.Key, tableDifference.IndexDifferences ?? new IndexDifference[0]))
                {
                    yield return script;
                }

                foreach (var script in defaultConstraintScriptBuilder.CreateUpgradeScripts(tableDifference))
                {
                    yield return script;
                }

                foreach (var script in checkConstraintScriptBuilder.CreateUpgradeScripts(tableDifference))
                {
                    yield return script;
                }

                if (tableDifference.DescriptionChangedTo != null)
                {
                    yield return descriptionScriptBuilder.ChangeTableDescription(
                        tableDifference.RequiredTable.Key,
                        tableDifference.CurrentTable.Value.Description,
                        tableDifference.DescriptionChangedTo.Value);
                }
            }
        }

        private SqlScript GetDropTableScript(TableName table)
        {
            return new SqlScript(@$"
DROP TABLE {table.SqlSafeName()}
GO");
        }

        private IEnumerable<SqlScript> GetAlterTableScript(TableDifference tableDifference)
        {
            var requiredTableName = tableDifference.RequiredTable.Key;
            var columnDifferences = tableDifference.ColumnDifferences;
            foreach (var rename in columnDifferences.Where(difference => difference.ColumnRenamedTo != null))
            {
                var requiredColumn = rename.RequiredColumn;
                var currentColumn = rename.CurrentColumn;
                yield return GetRenameColumnScript(requiredTableName, currentColumn.Key, requiredColumn.Key);
            }

            var drops = columnDifferences.Where(diff => diff.ColumnDeleted).ToArray();
            if (drops.Any())
            {
                yield return new SqlScript($@"ALTER TABLE {requiredTableName.SqlSafeName()}
{string.Join(",", drops.Select(col => $"DROP COLUMN {col.CurrentColumn.SqlSafeName()}"))}
GO");
            }

            foreach (var add in columnDifferences.Where(diff => diff.ColumnAdded))
            {
                foreach (var script in GetAddColumnScript(requiredTableName, add.RequiredColumn.Key, add.RequiredColumn.Value))
                    yield return script;

                if (add.DescriptionChangedTo != null)
                {
                    yield return descriptionScriptBuilder.ChangeColumnDescription(
                        requiredTableName,
                        add.RequiredColumn.Key,
                        null,
                        add.DescriptionChangedTo.Value);
                }
            }

            var alterations = columnDifferences.Where(IsAlteration);
            foreach (var alteration in alterations)
            {
                foreach (var script in GetAlterColumnScript(requiredTableName, alteration))
                {
                    yield return script;
                }
            }
        }

        private bool IsAlteration(ColumnDifference difference)
        {
            return !difference.ColumnAdded && !difference.ColumnDeleted && difference.IsChanged;
        }

        private IEnumerable<SqlScript> GetAlterColumnScript(TableName tableName, ColumnDifference columnDifference)
        {
            var column = columnDifference.RequiredColumn.Value;
            var columnName = columnDifference.RequiredColumn.Key;

            if (columnDifference.NullabilityChangedTo != null || columnDifference.TypeChangedTo != null)
            {
                //type of nullability has changed
                var nullabilityClause = column.Nullable == true
                    ? ""
                    : " NOT NULL";

                yield return new SqlScript($@"ALTER TABLE {tableName.SqlSafeName()}
ALTER COLUMN {columnName.SqlSafeName()} {column.Type}{nullabilityClause}
GO");
            }

            foreach (var script in defaultConstraintScriptBuilder.CreateUpgradeScripts(tableName, columnDifference))
            {
                yield return script;
            }

            foreach (var script in checkConstraintScriptBuilder.CreateUpgradeScripts(tableName, columnDifference))
            {
                yield return script;
            }

            if (columnDifference.DescriptionChangedTo != null)
            {
                yield return descriptionScriptBuilder.ChangeColumnDescription(
                    tableName, 
                    columnName, 
                    columnDifference.CurrentColumn.Value.Description, 
                    columnDifference.DescriptionChangedTo.Value);
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
                //NOTE: Required.Schema is used here as the table will have been moved between schemas before any table changes are executed

                yield return new SqlScript(@$"EXEC sp_rename 
    @objname = '{required.Schema}.{current.Table}', 
    @newname = '{required.Table}', 
    @objtype = 'OBJECT'
GO");
            }
        }

        private IEnumerable<SqlScript> GetCreateTableScript(TableDetails requiredTable, TableName tableName)
        {
            var columns = requiredTable.Columns.Select(CreateTableColumn);
            yield return new SqlScript($@"CREATE TABLE {tableName.SqlSafeName()} (
{string.Join("," + Environment.NewLine, columns)}
)
GO");

            if (requiredTable.Description != null)
            {
                yield return descriptionScriptBuilder.ChangeTableDescription(
                    tableName,
                    null,
                    requiredTable.Description);
            }

            foreach (var column in requiredTable.Columns.Where(column => column.Value.Description != null))
            {
                yield return descriptionScriptBuilder.ChangeColumnDescription(
                    tableName, 
                    column.Key, 
                    null, 
                    column.Value.Description);
            }
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
