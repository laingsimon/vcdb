using System.Collections.Generic;
using System.Linq;
using vcdb.Models;

namespace vcdb.Scripting
{
    public class ColumnComparer : IColumnComparer
    {
        private readonly INamedItemFinder namedItemFinder;

        public ColumnComparer(INamedItemFinder namedItemFinder)
        {
            this.namedItemFinder = namedItemFinder;
        }

        public IEnumerable<ColumnDifference> GetDifferentColumns(
            IDictionary<string, ColumnDetails> currentColumns,
            IDictionary<string, ColumnDetails> requiredColumns)
        {
            var processedColumns = new HashSet<ColumnDetails>();
            foreach (var requiredColumn in requiredColumns)
            {
                var currentColumn = namedItemFinder.GetCurrentItem(currentColumns, requiredColumn);

                if (currentColumn == null)
                {
                    yield return new ColumnDifference
                    {
                        RequiredColumn = requiredColumn.AsNamedItem(),
                        ColumnAdded = true
                    };
                }
                else
                {
                    processedColumns.Add(currentColumn.Value);

                    var difference = new ColumnDifference
                    {
                        CurrentColumn = currentColumn,
                        RequiredColumn = requiredColumn.AsNamedItem(),
                        ColumnRenamedTo = !currentColumn.Key.Equals(requiredColumn.Key)
                            ? requiredColumn.Key
                            : null
                    };

                    var columnDetailDifferences = ColumnsAreIdentical(currentColumn.Value, requiredColumn.Value);
                    if (columnDetailDifferences != null)
                        yield return difference.MergeIn(columnDetailDifferences);
                    else if (difference.IsChanged)
                        yield return difference;
                }
            }

            foreach (var currentColumn in currentColumns.Where(col => !processedColumns.Contains(col.Value)))
            {
                yield return new ColumnDifference
                {
                    CurrentColumn = currentColumn.AsNamedItem(),
                    ColumnDeleted = true
                };
            }
        }

        private ColumnDifference ColumnsAreIdentical(
            ColumnDetails currentColumn,
            ColumnDetails requiredColumn)
        {
            var defaultsIdentical = ColumnDefaultsAreIdentical(currentColumn.Default, requiredColumn.Default);

            var difference = new ColumnDifference
            {
                TypeChangedTo = currentColumn.Type == requiredColumn.Type
                    ? null
                    : requiredColumn.Type,
                NullabilityChangedTo = currentColumn.Nullable == requiredColumn.Nullable
                    ? null
                    : requiredColumn.Nullable,
                DefaultChangedTo = defaultsIdentical
                    ? ColumnDifference.UnchangedDefault
                    : requiredColumn.Default,
                DefaultRenamedTo = currentColumn.DefaultName == requiredColumn.DefaultName
                    ? null
                    : requiredColumn.DefaultName,
                DescriptionChangedTo = currentColumn.Description != requiredColumn.Description
                    ? requiredColumn.Description
                    : ColumnDifference.UnchangedDescription
            };

            return difference.IsChanged
                ? difference
                : null;
        }

        private bool ColumnDefaultsAreIdentical(object currentDefault, object requiredDefault)
        {
            if (currentDefault == null && requiredDefault == null)
                return true;

            if (currentDefault != null && requiredDefault != null)
            {
                if (requiredDefault is string requiredDefaultString && currentDefault is string currentDefaultString)
                {
                    return requiredDefaultString.Equals(currentDefaultString.Trim('\''));
                }

                if (requiredDefault.GetType().Equals(currentDefault.GetType()))
                    return currentDefault.Equals(requiredDefault);
                else if (requiredDefault is string)
                    return requiredDefault.Equals(currentDefault.ToString());
                else if (currentDefault is string)
                    return currentDefault.Equals(requiredDefault.ToString());
                else
                    return requiredDefault.ToString().Equals(currentDefault.ToString());
            }

            return false; //one has a value but not both
        }
    }
}
