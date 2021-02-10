using System;
using System.Collections.Generic;
using System.Linq;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;
using vcdb.Scripting.CheckConstraint;
using vcdb.Scripting.Column;
using vcdb.Scripting.ExecutionPlan;
using vcdb.Scripting.ForeignKey;
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
        private readonly IForeignKeyScriptBuilder foreignKeyScriptBuilder;

        public SqlServerTableScriptBuilder(
            IDescriptionScriptBuilder descriptionScriptBuilder,
            IIndexScriptBuilder indexScriptBuilder,
            IDefaultConstraintScriptBuilder defaultConstraintScriptBuilder,
            ICheckConstraintScriptBuilder checkConstraintScriptBuilder,
            IPrimaryKeyScriptBuilder primaryKeyScriptBuilder,
            IPermissionScriptBuilder permissionScriptBuilder,
            IForeignKeyScriptBuilder foreignKeyScriptBuilder)
        {
            this.descriptionScriptBuilder = descriptionScriptBuilder;
            this.indexScriptBuilder = indexScriptBuilder;
            this.defaultConstraintScriptBuilder = defaultConstraintScriptBuilder;
            this.checkConstraintScriptBuilder = checkConstraintScriptBuilder;
            this.primaryKeyScriptBuilder = primaryKeyScriptBuilder;
            this.permissionScriptBuilder = permissionScriptBuilder;
            this.foreignKeyScriptBuilder = foreignKeyScriptBuilder;
        }

        public IEnumerable<IScriptTask> CreateUpgradeScripts(IReadOnlyCollection<TableDifference> tableDifferences)
        {
            var processedDifferences = new List<TableDifference>();

            foreach (var tableDifference in tableDifferences)
            {
                var requiredTable = tableDifference.RequiredTable;
                var currentTable = tableDifference.CurrentTable;

                if (tableDifference.TableDeleted)
                {
                    processedDifferences.Add(tableDifference);
                    yield return GetDropTableScript(currentTable.Key);
                    continue;
                }

                if (tableDifference.TableAdded)
                {
                    processedDifferences.Add(tableDifference);
                    yield return new MultiScriptTask(GetCreateTableScripts(requiredTable.Value, requiredTable.Key, tableDifference));
                    continue;
                }

                if (tableDifference.ColumnDifferences?.Any() == true)
                {
                    var drops = tableDifference.ColumnDifferences.Where(diff => diff.ColumnDeleted || RequiresDropAndReAdd(diff)).ToArray();
                    if (drops.Any())
                    {
                        yield return DropsColumns(new SqlScript($@"ALTER TABLE {currentTable.Key.SqlSafeName()}
{string.Join(",", drops.Select(col => $"DROP COLUMN {col.CurrentColumn.SqlSafeName()}"))}
GO"), currentTable.Key, drops);
                    }
                }

                if (tableDifference.TableRenamedTo != null)
                {
                    yield return new MultiScriptTask(GetRenameTableScript(currentTable.Key, requiredTable.Key));
                }

                foreach (var rename in tableDifference.ColumnDifferences.Where(difference => difference.ColumnRenamedTo != null && !RequiresDropAndReAdd(difference)))
                {
                    var requiredColumn = rename.RequiredColumn;
                    var currentColumn = rename.CurrentColumn;
                    yield return GetRenameColumnScript(requiredTable.Key, currentColumn.Key, requiredColumn.Key);
                }


                yield return new MultiScriptTask(GetAlterTableScript(tableDifference));

                if (tableDifference.DescriptionChangedTo != null)
                {
                    yield return descriptionScriptBuilder.ChangeTableDescription(
                        tableDifference.RequiredTable.Key,
                        tableDifference.CurrentTable.Value.Description,
                        tableDifference.DescriptionChangedTo.Value);
                }
            }
        }

        private IScriptTask DropsColumns(SqlScript sqlScript, ObjectName tableName, IEnumerable<ColumnDifference> droppedColumns)
        {
            return droppedColumns.Select(col => col.CurrentColumn.Key).Aggregate(
                sqlScript.AsTask(),
                (task, droppedColumnName) => task.ResultingIn(new ScriptTaskDependency(DependencyAction.Drop, tableName.Component(droppedColumnName))));
        }

        private IScriptTask GetDropTableScript(ObjectName table)
        {
            return new SqlScript(@$"
DROP TABLE {table.SqlSafeName()}
GO").Drops().Table(table)
.Requiring().CheckConstraintOn(table.Components()).ToBeDropped();
        }

        private IEnumerable<IScriptTask> GetAlterTableScript(TableDifference tableDifference)
        {
            var requiredTableName = tableDifference.RequiredTable.Key;
            var columnDifferences = tableDifference.ColumnDifferences;

            foreach (var add in columnDifferences.Where(diff => diff.ColumnAdded || RequiresDropAndReAdd(diff)))
            {
                yield return GetAddColumnScript(requiredTableName, add.RequiredColumn.Key, add.RequiredColumn.Value);

                if (add.DescriptionChangedTo != null)
                {
                    yield return descriptionScriptBuilder.ChangeColumnDescription(
                        requiredTableName,
                        add.RequiredColumn.Key,
                        null,
                        add.DescriptionChangedTo.Value);
                }

                yield return new MultiScriptTask(permissionScriptBuilder.CreateColumnPermissionScripts(
                    requiredTableName,
                    add.RequiredColumn.Key,
                    PermissionDifferences.From(add.RequiredColumn.Value.Permissions)));
            }

            var alterations = columnDifferences.Where(IsAlteration);
            foreach (var alteration in alterations)
            {
                yield return new MultiScriptTask(GetAlterColumnScript(requiredTableName, alteration));
            }

            yield return new MultiScriptTask(indexScriptBuilder.CreateUpgradeScripts(requiredTableName, tableDifference.IndexDifferences.OrEmptyCollection()));
            yield return new MultiScriptTask(defaultConstraintScriptBuilder.CreateUpgradeScripts(tableDifference));
            yield return new MultiScriptTask(checkConstraintScriptBuilder.CreateUpgradeScripts(tableDifference));
            yield return new MultiScriptTask(primaryKeyScriptBuilder.CreateUpgradeScripts(requiredTableName, tableDifference.PrimaryKeyDifference));
            yield return new MultiScriptTask(foreignKeyScriptBuilder.CreateUpgradeScripts(requiredTableName, tableDifference.ForeignKeyDifferences));
            yield return new MultiScriptTask(permissionScriptBuilder.CreateTablePermissionScripts(
                requiredTableName,
                tableDifference.PermissionDifferences));
        }

        private bool RequiresDropAndReAdd(ColumnDifference difference)
        {
            return difference.ComputedChangedTo != null
                || difference.ExpressionChangedTo != null;
        }

        private bool IsAlteration(ColumnDifference difference)
        {
            return !difference.ColumnAdded 
                && !difference.ColumnDeleted 
                && difference.IsChanged
                && !RequiresDropAndReAdd(difference);
        }

        private IEnumerable<IScriptTask> GetAlterColumnScript(
            ObjectName tableName, 
            ColumnDifference columnDifference)
        {
            var column = columnDifference.RequiredColumn.Value;
            var columnName = columnDifference.RequiredColumn.Key;

            if (columnDifference.NullabilityChangedTo != null || columnDifference.TypeChangedTo != null || columnDifference.CollationChangedTo != null)
            {
                yield return new SqlScript($@"ALTER TABLE {tableName.SqlSafeName()}
ALTER COLUMN {ColumnClause(columnName, column, columnDifference.CollationChangedTo)}
GO").CreatesOrAlters().Columns(tableName.Component(columnName))
.Requiring().CheckConstraintOn(tableName.Component(columnName)).ToBeDropped();
            }

            yield return new MultiScriptTask(defaultConstraintScriptBuilder.CreateUpgradeScripts(tableName, columnDifference));

            if (columnDifference.DescriptionChangedTo != null)
            {
                yield return descriptionScriptBuilder.ChangeColumnDescription(
                    tableName, 
                    columnName, 
                    columnDifference.CurrentColumn.Value.Description, 
                    columnDifference.DescriptionChangedTo.Value);
            }

            yield return new MultiScriptTask(permissionScriptBuilder.CreateColumnPermissionScripts(
                tableName,
                columnName,
                columnDifference.PermissionDifferences));
        }

        private IScriptTask GetRenameColumnScript(
            ObjectName tableName,
            string currentColumnName,
            string requiredColumnName)
        {
            return new SqlScript(@$"EXEC sp_rename
    @objname = '{tableName.Schema}.{tableName.Name}.{currentColumnName}',
    @newname = '{requiredColumnName}',
    @objtype = 'COLUMN'
GO").CreatesOrAlters().Columns(tableName.Component(requiredColumnName))
.Drops().Columns(tableName.Component(currentColumnName))
.Requiring().CheckConstraintOn(tableName.Component(currentColumnName)).ToBeDropped()
.Requiring().ForeignKeyOn(tableName.Component(currentColumnName)).ToBeDropped()
.Requiring().ForeignKeyReferencing(tableName).ToBeDropped()
.Requiring().PrimaryKeyOn(tableName).ToBeDropped();
        }

        private IEnumerable<IScriptTask> GetRenameTableScript(ObjectName current, ObjectName required)
        {
            if (current.Name != required.Name)
            {
                //NOTE: Required.Schema is used here as the table will have been moved between schemas before any table changes are executed

                yield return new SqlScript(@$"EXEC sp_rename 
    @objname = '{required.Schema}.{current.Name}', 
    @newname = '{required.Name}', 
    @objtype = 'OBJECT'
GO").CreatesOrAlters().Table(required);
            }
        }

        private IEnumerable<IScriptTask> GetCreateTableScripts(TableDetails requiredTable, ObjectName tableName, TableDifference tableDifference)
        {
            var columns = requiredTable.Columns.Select(pair => ColumnClause(pair.Key, pair.Value));
            yield return new SqlScript($@"CREATE TABLE {tableName.SqlSafeName()} (
{string.Join("," + Environment.NewLine, columns)}
)
GO").CreatesOrAlters().Table(tableName);

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
                    yield return new MultiScriptTask(permissionScriptBuilder.CreateColumnPermissionScripts(
                        tableName,
                        column.Key,
                        PermissionDifferences.From(column.Value.Permissions)));
                }
            }

            yield return new MultiScriptTask(indexScriptBuilder.CreateUpgradeScripts(tableName, tableDifference.IndexDifferences.OrEmptyCollection()));
            yield return new MultiScriptTask(defaultConstraintScriptBuilder.CreateUpgradeScripts(tableDifference));
            yield return new MultiScriptTask(checkConstraintScriptBuilder.CreateUpgradeScripts(tableDifference));
            yield return new MultiScriptTask(primaryKeyScriptBuilder.CreateUpgradeScripts(tableName, tableDifference.PrimaryKeyDifference));
            yield return new MultiScriptTask(foreignKeyScriptBuilder.CreateUpgradeScripts(tableName, tableDifference.ForeignKeyDifferences));
            yield return new MultiScriptTask(permissionScriptBuilder.CreateTablePermissionScripts(
                tableName,
                PermissionDifferences.From(requiredTable.Permissions)));

            if (tableDifference.DescriptionChangedTo != null)
            {
                yield return descriptionScriptBuilder.ChangeTableDescription(
                    tableName,
                    tableDifference.CurrentTable.Value.Description,
                    tableDifference.DescriptionChangedTo.Value);
            }
        }

        private IScriptTask GetAddColumnScript(ObjectName tableName, string columnName, ColumnDetails column)
        {
            return new SqlScript($@"ALTER TABLE {tableName.SqlSafeName()}
ADD {ColumnClause(columnName, column)}
GO").CreatesOrAlters().Columns(tableName.Component(columnName));
        }

        private string ColumnClause(string columnName, ColumnDetails column, string collationOverride = null)
        {
            var nullabilityClause = column.Nullable == true
                ? ""
                : " NOT NULL";
            var collationClause = (collationOverride ?? column.Collation) != null
                ? $" COLLATE {(collationOverride ?? column.Collation)}"
                : null;

            if (string.IsNullOrEmpty(column.Expression))
            {
                return $"{columnName.SqlSafeName()} {column.Type}{collationClause}{nullabilityClause}";
            }

            return $"{columnName.SqlSafeName()} AS ({column.Expression})";
        }
    }
}
