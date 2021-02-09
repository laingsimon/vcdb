﻿using System;
using System.Collections.Generic;
using System.Linq;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;
using vcdb.Scripting.CheckConstraint;
using vcdb.Scripting.Column;
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

        public IEnumerable<IOutputable> CreateUpgradeScripts(IReadOnlyCollection<TableDifference> tableDifferences)
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
                    yield return new OutputableCollection(GetCreateTableScript(requiredTable.Value, requiredTable.Key));
                    yield return new OutputableCollection(GetCreateTableScripts(tableDifference));

                    continue;
                }

                yield return new OutputableCollection(foreignKeyScriptBuilder.CreateUpgradeScripts(currentTable.Key, tableDifference.ForeignKeyDifferences, ScriptingPhase.DropReferences));
            }

            //Drop dependencies phase
            foreach (var tableDifference in tableDifferences.Where(diff => diff.CurrentTable != null))
            {
                var requiredTable = tableDifference.RequiredTable;
                var currentTable = tableDifference.CurrentTable;

                yield return new OutputableCollection(primaryKeyScriptBuilder.CreateUpgradeScripts(currentTable.Key, tableDifference.PrimaryKeyDifference, ScriptingPhase.DropDependencies));

                if (tableDifference.ColumnDifferences?.Any() == true)
                {
                    var drops = tableDifference.ColumnDifferences.Where(diff => diff.ColumnDeleted || RequiresDropAndReAdd(diff)).ToArray();
                    if (drops.Any())
                    {
                        yield return new SqlScript($@"ALTER TABLE {currentTable.Key.SqlSafeName()}
{string.Join(",", drops.Select(col => $"DROP COLUMN {col.CurrentColumn.SqlSafeName()}"))}
GO");
                    }
                }
            }

            //Alter phase
            foreach (var tableDifference in tableDifferences.Except(processedDifferences))
            {
                var requiredTable = tableDifference.RequiredTable;
                var currentTable = tableDifference.CurrentTable;

                if (tableDifference.TableRenamedTo != null)
                {
                    yield return new OutputableCollection(GetRenameTableScript(currentTable.Key, requiredTable.Key));
                }

                foreach (var rename in tableDifference.ColumnDifferences.Where(difference => difference.ColumnRenamedTo != null && !RequiresDropAndReAdd(difference)))
                {
                    yield return new OutputableCollection(checkConstraintScriptBuilder.CreateUpgradeScriptsBeforeColumnChanges(tableDifference, rename));

                    var requiredColumn = rename.RequiredColumn;
                    var currentColumn = rename.CurrentColumn;
                    yield return new OutputableCollection(GetRenameColumnScript(requiredTable.Key, currentColumn.Key, requiredColumn.Key));
                }

                yield return new OutputableCollection(GetAlterTableScript(tableDifference));
            }

            //Recreate depdencies phase
            foreach (var tableDifference in tableDifferences.Except(processedDifferences))
            {
                yield return new OutputableCollection(primaryKeyScriptBuilder.CreateUpgradeScripts(tableDifference.RequiredTable.Key, tableDifference.PrimaryKeyDifference, ScriptingPhase.RecreateDependencies));
            }

            //Recreate phase
            foreach (var tableDifference in tableDifferences.Except(processedDifferences))
            {
                yield return new OutputableCollection(GetChangeTableScripts(tableDifference));
            }
        }

        private IEnumerable<IOutputable> GetCreateTableScripts(TableDifference tableDifference)
        {
            var requiredTable = tableDifference.RequiredTable;

            yield return new OutputableCollection(indexScriptBuilder.CreateUpgradeScripts(requiredTable.Key, tableDifference.IndexDifferences.OrEmptyCollection()));
            yield return new OutputableCollection(defaultConstraintScriptBuilder.CreateUpgradeScripts(tableDifference));
            yield return new OutputableCollection(checkConstraintScriptBuilder.CreateUpgradeScripts(tableDifference));
            yield return new OutputableCollection(primaryKeyScriptBuilder.CreateUpgradeScripts(requiredTable.Key, tableDifference.PrimaryKeyDifference, ScriptingPhase.Recreate));
            yield return new OutputableCollection(foreignKeyScriptBuilder.CreateUpgradeScripts(requiredTable.Key, tableDifference.ForeignKeyDifferences, ScriptingPhase.Recreate));
            yield return new OutputableCollection(permissionScriptBuilder.CreateTablePermissionScripts(
                requiredTable.Key,
                PermissionDifferences.From(requiredTable.Value.Permissions)));
        }

        private IEnumerable<IOutputable> GetChangeTableScripts(TableDifference tableDifference)
        {
            var requiredTable = tableDifference.RequiredTable;

            yield return new OutputableCollection(indexScriptBuilder.CreateUpgradeScripts(requiredTable.Key, tableDifference.IndexDifferences.OrEmptyCollection()));
            yield return new OutputableCollection(defaultConstraintScriptBuilder.CreateUpgradeScripts(tableDifference)); 
            yield return new OutputableCollection(checkConstraintScriptBuilder.CreateUpgradeScripts(tableDifference)); 
            yield return new OutputableCollection(primaryKeyScriptBuilder.CreateUpgradeScripts(requiredTable.Key, tableDifference.PrimaryKeyDifference, ScriptingPhase.Recreate)); 
            yield return new OutputableCollection(foreignKeyScriptBuilder.CreateUpgradeScripts(requiredTable.Key, tableDifference.ForeignKeyDifferences, ScriptingPhase.Recreate)); 

            if (tableDifference.DescriptionChangedTo != null)
            {
                yield return descriptionScriptBuilder.ChangeTableDescription(
                    tableDifference.RequiredTable.Key,
                    tableDifference.CurrentTable.Value.Description,
                    tableDifference.DescriptionChangedTo.Value);
            }

            yield return new OutputableCollection(permissionScriptBuilder.CreateTablePermissionScripts(
                requiredTable.Key,
                tableDifference.PermissionDifferences)); 
        }

        private SqlScript GetDropTableScript(ObjectName table)
        {
            return new SqlScript(@$"
DROP TABLE {table.SqlSafeName()}
GO");
        }

        private IEnumerable<IOutputable> GetAlterTableScript(TableDifference tableDifference)
        {
            var requiredTableName = tableDifference.RequiredTable.Key;
            var columnDifferences = tableDifference.ColumnDifferences;

            foreach (var add in columnDifferences.Where(diff => diff.ColumnAdded || RequiresDropAndReAdd(diff)))
            {
                yield return new OutputableCollection(GetAddColumnScript(requiredTableName, add.RequiredColumn.Key, add.RequiredColumn.Value));

                if (add.DescriptionChangedTo != null)
                {
                    yield return descriptionScriptBuilder.ChangeColumnDescription(
                        requiredTableName,
                        add.RequiredColumn.Key,
                        null,
                        add.DescriptionChangedTo.Value);
                }

                yield return new OutputableCollection(permissionScriptBuilder.CreateColumnPermissionScripts(
                    requiredTableName,
                    add.RequiredColumn.Key,
                    PermissionDifferences.From(add.RequiredColumn.Value.Permissions)));
            }

            var alterations = columnDifferences.Where(IsAlteration);
            foreach (var alteration in alterations)
            {
                yield return new OutputableCollection(GetAlterColumnScript(requiredTableName, alteration));
            }
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

        private IEnumerable<IOutputable> GetAlterColumnScript(
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

            yield return new OutputableCollection(defaultConstraintScriptBuilder.CreateUpgradeScripts(tableName, columnDifference));

            if (columnDifference.DescriptionChangedTo != null)
            {
                yield return descriptionScriptBuilder.ChangeColumnDescription(
                    tableName, 
                    columnName, 
                    columnDifference.CurrentColumn.Value.Description, 
                    columnDifference.DescriptionChangedTo.Value);
            }

            yield return new OutputableCollection(permissionScriptBuilder.CreateColumnPermissionScripts(
                tableName,
                columnName,
                columnDifference.PermissionDifferences));
        }

        private SqlScript GetRenameColumnScript(
            ObjectName tableName,
            string currentColumnName,
            string requiredColumnName)
        {
            //TODO: If there are anly check constraints bound to this column, drop it first.

            return new SqlScript(@$"EXEC sp_rename
    @objname = '{tableName.Schema}.{tableName.Name}.{currentColumnName}',
    @newname = '{requiredColumnName}',
    @objtype = 'COLUMN'
GO");
        }

        private IEnumerable<IOutputable> GetRenameTableScript(ObjectName current, ObjectName required)
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

        private IEnumerable<IOutputable> GetCreateTableScript(TableDetails requiredTable, ObjectName tableName)
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
                    yield return new OutputableCollection(permissionScriptBuilder.CreateColumnPermissionScripts(
                        tableName,
                        column.Key,
                        PermissionDifferences.From(column.Value.Permissions)));
                }
            }
        }

        private SqlScript GetAddColumnScript(ObjectName tableName, string columnName, ColumnDetails column)
        {
            return new SqlScript($@"ALTER TABLE {tableName.SqlSafeName()}
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

            if (string.IsNullOrEmpty(column.Expression))
            {
                return $"{columnName.SqlSafeName()} {column.Type}{collationClause}{nullabilityClause}";
            }

            return $"{columnName.SqlSafeName()} AS ({column.Expression})";
        }
    }
}
