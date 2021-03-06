﻿using System.Collections.Generic;
using System.Linq;
using vcdb.Models;
using vcdb.Scripting.CheckConstraint;
using vcdb.Scripting.Column;
using vcdb.Scripting.ForeignKey;
using vcdb.Scripting.Index;
using vcdb.Scripting.Permission;
using vcdb.Scripting.PrimaryKey;

namespace vcdb.Scripting.Table
{
    public class TableComparer : ITableComparer
    {
        private readonly IColumnComparer columnComparer;
        private readonly IIndexComparer indexComparer;
        private readonly INamedItemFinder namedItemFinder;
        private readonly ICheckConstraintComparer checkConstraintComparer;
        private readonly IPrimaryKeyComparer primaryKeyComparer;
        private readonly IPermissionComparer permissionComparer;
        private readonly IForeignKeyComparer foreignKeyComparer;

        public TableComparer(
            IColumnComparer columnComparer,
            IIndexComparer indexComparer,
            INamedItemFinder namedItemFinder,
            ICheckConstraintComparer checkConstraintComparer,
            IPrimaryKeyComparer primaryKeyComparer,
            IPermissionComparer permissionComparer,
            IForeignKeyComparer foreignKeyComparer)
        {
            this.columnComparer = columnComparer;
            this.indexComparer = indexComparer;
            this.namedItemFinder = namedItemFinder;
            this.checkConstraintComparer = checkConstraintComparer;
            this.primaryKeyComparer = primaryKeyComparer;
            this.permissionComparer = permissionComparer;
            this.foreignKeyComparer = foreignKeyComparer;
        }

        public IEnumerable<TableDifference> GetTableDifferences(
            ComparerContext context,
            IDictionary<ObjectName, TableDetails> currentTables,
            IDictionary<ObjectName, TableDetails> requiredTables)
        {
            var processedTables = new HashSet<TableDetails>();
            foreach (var requiredTable in requiredTables)
            {
                var currentTable = namedItemFinder.GetCurrentItem(currentTables, requiredTable);

                if (currentTable == null)
                {
                    yield return new TableDifference
                    {
                        RequiredTable = requiredTable.AsNamedItem(),
                        TableAdded = true
                    };
                }
                else
                {
                    processedTables.Add(currentTable.Value);

                    var columnDifferences = columnComparer.GetDifferentColumns(
                            context.ForTable(currentTable, requiredTable.AsNamedItem()),
                            currentTable.Value.Columns.OrEmpty(),
                            requiredTable.Value.Columns.OrEmpty()).ToArray();

                    var difference = new TableDifference
                    {
                        CurrentTable = currentTable,
                        RequiredTable = requiredTable.AsNamedItem(),
                        TableRenamedTo = !currentTable.Key.Equals(requiredTable.Key)
                            ? requiredTable.Key
                            : null,
                        ColumnDifferences = columnDifferences,
                        IndexDifferences = indexComparer.GetIndexDifferences(
                            context.ForTable(currentTable, requiredTable.AsNamedItem(), columnDifferences),
                            currentTable.Value.Indexes.OrEmpty(),
                            requiredTable.Value.Indexes.OrEmpty()).ToArray(),
                        DescriptionChangedTo = currentTable.Value.Description != requiredTable.Value.Description
                            ? requiredTable.Value.Description.AsChange()
                            : null,
                        ChangedCheckConstraints = checkConstraintComparer.GetDifferentCheckConstraints(
                            context.ForTable(currentTable, requiredTable.AsNamedItem(), columnDifferences),
                            currentTable.Value.Checks.OrEmptyCollection(),
                            requiredTable.Value.Checks.OrEmptyCollection()).ToArray(),
                        PrimaryKeyDifference = primaryKeyComparer.GetPrimaryKeyDifference(
                            context.ForTable(currentTable, requiredTable.AsNamedItem(), columnDifferences),
                            currentTable.Value, 
                            requiredTable.Value),
                        PermissionDifferences = permissionComparer.GetPermissionDifferences(
                            context.ForTable(currentTable, requiredTable.AsNamedItem(), columnDifferences),
                            currentTable.Value.Permissions,
                            requiredTable.Value.Permissions),
                        ForeignKeyDifferences = foreignKeyComparer.GetForeignKeyDifferences(
                            context.ForTable(currentTable, requiredTable.AsNamedItem(), columnDifferences),
                            currentTable.Value.ForeignKeys.OrEmpty(),
                            requiredTable.Value.ForeignKeys.OrEmpty()).ToArray()
                    };

                    if (difference.IsChanged)
                    {
                        yield return difference;
                    }
                }
            }

            foreach (var currentTable in currentTables.Where(col => !processedTables.Contains(col.Value)))
            {
                yield return new TableDifference
                {
                    CurrentTable = currentTable.AsNamedItem(),
                    TableDeleted = true
                };
            }
        }
    }
}
