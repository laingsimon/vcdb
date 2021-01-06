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

        public TableComparer(IColumnComparer columnComparer, IIndexComparer indexComparer, INamedItemFinder namedItemFinder)
        {
            this.columnComparer = columnComparer;
            this.indexComparer = indexComparer;
            this.namedItemFinder = namedItemFinder;
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

                    var difference = new TableDifference
                    {
                        CurrentTable = currentTable,
                        RequiredTable = requiredTable.AsNamedItem(),
                        TableRenamedTo = !currentTable.Key.Equals(requiredTable.Key)
                            ? requiredTable.Key
                            : null,
                        ColumnDifferences = columnComparer.GetDifferentColumns(currentTable.Value.Columns, requiredTable.Value.Columns).ToArray(),
                        IndexDifferences = indexComparer.GetIndexDifferences(currentTable.Value.Indexes, requiredTable.Value.Indexes, requiredTable.Value.Columns).ToArray(),
                        DescriptionChangedTo = currentTable.Value.Description != requiredTable.Value.Description
                            ? requiredTable.Value.Description
                            : TableDifference.UnchangedDescription
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
