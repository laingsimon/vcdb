using System;
using System.Collections.Generic;
using System.Linq;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;
using vcdb.Scripting.CheckConstraint;
using vcdb.Scripting.Column;
using vcdb.Scripting.Index;
using vcdb.Scripting.Permission;
using vcdb.Scripting.PrimaryKey;
using vcdb.Scripting.Table;

namespace vcdb.SqlServer.Scripting
{
    public class SqlServerTableScriptBuilder : ITableScriptBuilder
    {
        private readonly IDescriptionScriptBuilder descriptionScriptBuilder;
        private readonly IIndexScriptBuilder indexScriptBuilder;
        private readonly IDefaultConstraintScriptBuilder defaultConstraintScriptBuilder;
        private readonly ICheckConstraintScriptBuilder checkConstraintScriptBuilder;
        private readonly IPrimaryKeyScriptBuilder primaryKeyScriptBuilder;
        private readonly IPermissionScriptBuilder permissionScriptBuilder;

        public SqlServerTableScriptBuilder(
            IDescriptionScriptBuilder descriptionScriptBuilder,
            IIndexScriptBuilder indexScriptBuilder,
            IDefaultConstraintScriptBuilder defaultConstraintScriptBuilder,
            ICheckConstraintScriptBuilder checkConstraintScriptBuilder,
            IPrimaryKeyScriptBuilder primaryKeyScriptBuilder,
            IPermissionScriptBuilder permissionScriptBuilder)
        {
            this.descriptionScriptBuilder = descriptionScriptBuilder;
            this.indexScriptBuilder = indexScriptBuilder;
            this.defaultConstraintScriptBuilder = defaultConstraintScriptBuilder;
            this.checkConstraintScriptBuilder = checkConstraintScriptBuilder;
            this.primaryKeyScriptBuilder = primaryKeyScriptBuilder;
            this.permissionScriptBuilder = permissionScriptBuilder;
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

                    foreach (var script in primaryKeyScriptBuilder.CreateUpgradeScripts(requiredTable.Key, tableDifference.PrimaryKeyDifference))
                    {
                        yield return script;
                    }

                    foreach (var script in permissionScriptBuilder.CreateTablePermissionScripts(
                        requiredTable.Key,
                        PermissionDifferences.From(requiredTable.Value.Permissions)))
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

                foreach (var script in primaryKeyScriptBuilder.CreateUpgradeScripts(requiredTable.Key, tableDifference.PrimaryKeyDifference))
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

                foreach (var script in permissionScriptBuilder.CreateTablePermissionScripts(
                    requiredTable.Key,
                    tableDifference.PermissionDifferences))
                {
                    yield return script;
                }
            }
        }

        private SqlScript GetDropTableScript(ObjectName table)
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
                foreach (var script in checkConstraintScriptBuilder.CreateUpgradeScriptsBeforeColumnChanges(tableDifference, rename))
                {
                    yield return script;
                }

                var requiredColumn = rename.RequiredColumn;
                var currentColumn = rename.CurrentColumn;
                foreach (var script in GetRenameColumnScript(requiredTableName, currentColumn.Key, requiredColumn.Key))
                {
                    yield return script;
                }
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

                foreach (var script in permissionScriptBuilder.CreateColumnPermissionScripts(
                    requiredTableName,
                    add.RequiredColumn.Key,
                    PermissionDifferences.From(add.RequiredColumn.Value.Permissions)))
                {
                    yield return script;
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

        private IEnumerable<SqlScript> GetAlterColumnScript(
            ObjectName tableName, 
            ColumnDifference columnDifference)
        {
            var column = columnDifference.RequiredColumn.Value;
            var columnName = columnDifference.RequiredColumn.Key;

            if (columnDifference.NullabilityChangedTo != null || columnDifference.TypeChangedTo != null || columnDifference.CollationChangedTo != null)
            {
                yield return new SqlScript($@"ALTER TABLE {tableName.SqlSafeName()}
ALTER COLUMN {ColumnClause(columnName, column, columnDifference.CollationChangedTo)}
GO");
            }

            foreach (var script in defaultConstraintScriptBuilder.CreateUpgradeScripts(tableName, columnDifference))
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

            foreach (var script in permissionScriptBuilder.CreateColumnPermissionScripts(
                tableName,
                columnName,
                columnDifference.PermissionDifferences))
            {
                yield return script;
            }
        }

        private IEnumerable<SqlScript> GetRenameColumnScript(
            ObjectName tableName,
            string currentColumnName,
            string requiredColumnName)
        {
            //TODO: If there are anly check constraints bound to this column, drop it first.

            yield return new SqlScript(@$"EXEC sp_rename
    @objname = '{tableName.Schema}.{tableName.Name}.{currentColumnName}',
    @newname = '{requiredColumnName}',
    @objtype = 'COLUMN'
GO");
        }

        private IEnumerable<SqlScript> GetRenameTableScript(ObjectName current, ObjectName required)
        {
            if (current.Name != required.Name)
            {
                //NOTE: Required.Schema is used here as the table will have been moved between schemas before any table changes are executed

                yield return new SqlScript(@$"EXEC sp_rename 
    @objname = '{required.Schema}.{current.Name}', 
    @newname = '{required.Name}', 
    @objtype = 'OBJECT'
GO");
            }
        }

        private IEnumerable<SqlScript> GetCreateTableScript(TableDetails requiredTable, ObjectName tableName)
        {
            var columns = requiredTable.Columns.Select(pair => ColumnClause(pair.Key, pair.Value));
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

            foreach (var column in requiredTable.Columns)
            {
                if (column.Value.Description != null)
                {
                    yield return descriptionScriptBuilder.ChangeColumnDescription(
                        tableName,
                        column.Key,
                        null,
                        column.Value.Description);
                }

                if (column.Value.Permissions != null)
                {
                    foreach (var script in permissionScriptBuilder.CreateColumnPermissionScripts(
                        tableName,
                        column.Key,
                        PermissionDifferences.From(column.Value.Permissions)))
                    {
                        yield return script;
                    }
                }
            }
        }

        private IEnumerable<SqlScript> GetAddColumnScript(ObjectName tableName, string columnName, ColumnDetails column)
        {
            yield return new SqlScript($@"ALTER TABLE {tableName.SqlSafeName()}
ADD {ColumnClause(columnName, column)}
GO");
        }

        private string ColumnClause(string columnName, ColumnDetails column, string collationOverride = null)
        {
            var nullabilityClause = column.Nullable == true
                ? ""
                : " NOT NULL";
            var collationClause = (collationOverride ?? column.Collation) != null
                ? $" COLLATE {(collationOverride ?? column.Collation)}"
                : null;

            return $"{columnName.SqlSafeName()} {column.Type}{collationClause}{nullabilityClause}";
        }
    }
}
