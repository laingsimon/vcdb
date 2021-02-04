using System.Collections.Generic;
using System.Linq;
using vcdb.Models;
using vcdb.Scripting.Collation;
using vcdb.Scripting.Permission;

namespace vcdb.Scripting.Column
{
    public class ColumnComparer : IColumnComparer
    {
        private readonly INamedItemFinder namedItemFinder;
        private readonly ICollationComparer collationComparer;
        private readonly IPermissionComparer permissionComparer;

        public ColumnComparer(
            INamedItemFinder namedItemFinder,
            ICollationComparer collationComparer,
            IPermissionComparer permissionComparer)
        {
            this.namedItemFinder = namedItemFinder;
            this.collationComparer = collationComparer;
            this.permissionComparer = permissionComparer;
        }

        public IEnumerable<ColumnDifference> GetDifferentColumns(
            ComparerContext context,
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
                            : null,
                        PermissionDifferences = permissionComparer.GetPermissionDifferences(
                            context,
                            currentColumn.Value.Permissions,
                            requiredColumn.Value.Permissions)
                    };

                    var columnDetailDifferences = ColumnsAreIdentical(context, currentColumn.Value, requiredColumn.Value);
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
            ComparerContext context,
            ColumnDetails currentColumn,
            ColumnDetails requiredColumn)
        {
            var defaultsIdentical = ColumnDefaultsAreIdentical(currentColumn.Default, requiredColumn.Default);

            var difference = new ColumnDifference
            {
                TypeChangedTo = currentColumn.Type == requiredColumn.Type
                    ? null
                    : requiredColumn.Type,
                NullabilityChangedTo = currentColumn.Nullable != requiredColumn.Nullable
                    ? (requiredColumn.Nullable ?? OptOut.True).AsChange()
                    : null,
                DefaultChangedTo = defaultsIdentical
                    ? null
                    : requiredColumn.Default.AsChange(),
                DefaultRenamedTo = currentColumn.DefaultName != requiredColumn.DefaultName
                    ? requiredColumn.DefaultName.AsChange()
                    : null,
                DescriptionChangedTo = currentColumn.Description != requiredColumn.Description
                    ? requiredColumn.Description.AsChange()
                    : null,
                CollationChangedTo = collationComparer.GetColumnCollationChange(
                    context,
                    currentColumn,
                    requiredColumn)
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
