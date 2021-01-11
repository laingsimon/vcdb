using System.Collections.Generic;
using System.Linq;
using vcdb.Models;

namespace vcdb.Scripting
{
    public class TableComparer : ITableComparer
    {
        private readonly IColumnComparer columnComparer;
        private readonly IIndexComparer indexComparer;
        private readonly INamedItemFinder namedItemFinder;
        private readonly ICheckConstraintComparer checkConstraintComparer;

        public TableComparer(
            IColumnComparer columnComparer, 
            IIndexComparer indexComparer, 
            INamedItemFinder namedItemFinder,
            ICheckConstraintComparer checkConstraintComparer)
        {
            this.columnComparer = columnComparer;
            this.indexComparer = indexComparer;
            this.namedItemFinder = namedItemFinder;
            this.checkConstraintComparer = checkConstraintComparer;
        }

        public IEnumerable<TableDifference> GetDifferentTables(
            IDictionary<TableName, TableDetails> currentTables,
            IDictionary<TableName, TableDetails> requiredTables)
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
                            currentTable.Value.Indexes.OrEmpty(),
                            requiredTable.Value.Indexes.OrEmpty(),
                            requiredTable.Value.Columns.OrEmpty()).ToArray(),
                        DescriptionChangedTo = currentTable.Value.Description != requiredTable.Value.Description
                            ? requiredTable.Value.Description.AsChange()
                            : null,
                        ChangedCheckConstraints = checkConstraintComparer.GetDifferentCheckConstraints(
                            currentTable.Value.Checks.OrEmptyCollection(),
                            requiredTable.Value.Checks.OrEmptyCollection(),
                            columnDifferences).ToArray()
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
