using System.Linq;
using vcdb.Models;

namespace vcdb.Scripting.PrimaryKey
{
    public class PrimaryKeyComparer : IPrimaryKeyComparer
    {
        public PrimaryKeyDifference GetPrimaryKeyDifference(
            ComparerContext context,
            TableDetails currentTable,
            TableDetails requiredTable)
        {
            var currentPrimaryKeyColumns = currentTable?.Columns.Where(col => col.Value.PrimaryKey).Select(named => named.Key).ToArray() ?? new string[0];
            var requiredPrimaryKeyColumns = requiredTable?.Columns.Where(col => col.Value.PrimaryKey).Select(named => named.Key).ToArray() ?? new string[0];
            var currentPrimaryKey = currentTable?.PrimaryKey;
            var requiredPrimaryKey = requiredTable?.PrimaryKey;

            var difference = new PrimaryKeyDifference
            {
                CurrentPrimaryKey = currentPrimaryKey,
                RequiredPrimaryKey = requiredPrimaryKey,
                ColumnsRemoved = currentPrimaryKeyColumns?.Except(requiredPrimaryKeyColumns).ToArray(),
                ColumnsAdded = requiredPrimaryKeyColumns?.Except(currentPrimaryKeyColumns).ToArray(),
                RequiredColumns = requiredPrimaryKeyColumns,
                RenamedTo = currentPrimaryKey?.Name != requiredPrimaryKey?.Name
                    ? (requiredPrimaryKey?.Name ?? null).AsChange()
                    : null,
                ClusteredChangedTo = currentPrimaryKey?.Clustered != requiredPrimaryKey?.Clustered
                    ? (requiredPrimaryKey?.Clustered ?? OptOut.True).AsChange()
                    : null,
                PrimaryKeyAdded = currentPrimaryKeyColumns.Length == 0 && requiredPrimaryKeyColumns.Length > 0,
                PrimaryKeyRemoved = requiredPrimaryKeyColumns.Length == 0 && currentPrimaryKeyColumns.Length > 0,
                DescriptionChangedTo = currentPrimaryKey?.Description != requiredPrimaryKey?.Description
                    ? (requiredPrimaryKey?.Description ?? null).AsChange()
                    : null
            };

            return difference.IsChanged
                ? difference
                : null;
        }
    }
}
