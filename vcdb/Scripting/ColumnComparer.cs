using System.Collections.Generic;
using System.Linq;
using vcdb.Models;

namespace vcdb.Scripting
{
    public class ColumnComparer : IColumnComparer
    {
        public IEnumerable<ColumnDifference> GetDifferentColumns(
            Dictionary<string, ColumnDetails> currentColumns,
            Dictionary<string, ColumnDetails> requiredColumns)
        {
            var processedColumns = new HashSet<ColumnDetails>();
            foreach (var requiredColumn in requiredColumns)
            {
                var currentColumn = GetCurrentColumn(currentColumns, requiredColumn);

                if (currentColumn == null)
                {
                    yield return new ColumnDifference
                    {
                        RequiredColumn = requiredColumn.AsNamedItem()
                    };
                }
                else
                {
                    processedColumns.Add(currentColumn.Value);

                    if (!currentColumn.Key.Equals(requiredColumn.Key) || !ColumnsAreIdentical(currentColumn.Value, requiredColumn.Value))
                    {
                        yield return new ColumnDifference
                        {
                            CurrentColumn = currentColumn,
                            RequiredColumn = requiredColumn.AsNamedItem()
                        };
                    }
                }
            }

            foreach (var currentColumn in currentColumns.Where(col => !processedColumns.Contains(col.Value)))
            {
                yield return new ColumnDifference
                {
                    CurrentColumn = currentColumn.AsNamedItem()
                };
            }
        }

        private bool ColumnsAreIdentical(
            ColumnDetails currentColumnInst,
            ColumnDetails requiredColumn)
        {
            return currentColumnInst.Type == requiredColumn.Type
                && currentColumnInst.Nullable == requiredColumn.Nullable
                && ColumnDefaultsAreIdentical(currentColumnInst.Default, requiredColumn.Default);
        }

        private bool ColumnDefaultsAreIdentical(object currentDefault, object requiredDefault)
        {
            if (currentDefault == null && requiredDefault == null)
                return true;

            if (currentDefault != null && requiredDefault != null)
                return currentDefault.Equals(requiredDefault);

            return false; //one has a value but not both
        }

        private NamedItem<string, ColumnDetails> GetCurrentColumn(
            Dictionary<string, ColumnDetails> currentColumns,
            KeyValuePair<string, ColumnDetails> requiredColumn)
        {
            return GetCurrentColumn(currentColumns, requiredColumn.Key)
                ?? GetCurrentColumnForPreviousName(currentColumns, requiredColumn.Value.PreviousNames);
        }

        private NamedItem<string, ColumnDetails> GetCurrentColumn(
            IDictionary<string, ColumnDetails> currentColumns,
            string requiredColumnName)
        {
            var tablesWithSameName = currentColumns.Where(pair => pair.Key.Equals(requiredColumnName)).ToArray();
            return tablesWithSameName.Length == 1
                ? tablesWithSameName[0].AsNamedItem()
                : NamedItem<string, ColumnDetails>.Null;
        }

        private NamedItem<string, ColumnDetails> GetCurrentColumnForPreviousName(
            IDictionary<string, ColumnDetails> currentColumns,
            string[] previousNames)
        {
            if (previousNames == null)
                return null;

            return previousNames
                .Select(previousName => GetCurrentColumn(currentColumns, previousName))
                .FirstOrDefault(currentTable => currentTable != null);
        }
    }
}
