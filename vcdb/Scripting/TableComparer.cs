using System.Collections.Generic;
using System.Linq;
using vcdb.Models;

namespace vcdb.Scripting
{
    public class TableComparer : ITableComparer
    {
        private readonly IColumnComparer columnComparer;

        public TableComparer(IColumnComparer columnComparer)
        {
            this.columnComparer = columnComparer;
        }

        public IEnumerable<TableDifference> GetDifferentTables(
            IDictionary<TableName, TableDetails> currentTables,
            IDictionary<TableName, TableDetails> requiredTables)
        {
            var processedTables = new HashSet<TableDetails>();
            foreach (var requiredTable in requiredTables)
            {
                var currentTable = GetCurrentTable(currentTables, requiredTable);

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
                        ColumnDifferences = columnComparer.GetDifferentColumns(currentTable.Value.Columns, requiredTable.Value.Columns).ToArray()
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
                .FirstOrDefault(current => current != null);
        }
    }
}
