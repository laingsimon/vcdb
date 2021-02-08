using System.Collections.Generic;
using System.Linq;
using vcdb.Models;

namespace vcdb.Scripting.ForeignKey
{
    public class ForeignKeyComparer : IForeignKeyComparer
    {
        private readonly INamedItemFinder namedItemFinder;

        public ForeignKeyComparer(INamedItemFinder namedItemFinder)
        {
            this.namedItemFinder = namedItemFinder;
        }

        public IEnumerable<ForeignKeyDifference> GetForeignKeyDifferences(
            ComparerContext context,
            IDictionary<string, ForeignKeyDetails> currentForeignKeys,
            IDictionary<string, ForeignKeyDetails> requiredForeignKeys)
        {
            var requiredTableColumns = context.RequiredTable.Value.Columns;

            var processedForeignKeys = new HashSet<ForeignKeyDetails>();
            foreach (var requiredForeignKey in requiredForeignKeys)
            {
                var currentForeignKey = namedItemFinder.GetCurrentItem(currentForeignKeys, requiredForeignKey);

                if (currentForeignKey == null)
                {
                    yield return new ForeignKeyDifference
                    {
                        RequiredForeignKey = requiredForeignKey.AsNamedItem(),
                        ForeignKeyAdded = true
                    };
                }
                else
                {
                    processedForeignKeys.Add(currentForeignKey.Value);

                    var difference = new ForeignKeyDifference
                    {
                        CurrentForeignKey = currentForeignKey,
                        RequiredForeignKey = requiredForeignKey.AsNamedItem(),
                        ForeignKeyRenamedTo = !currentForeignKey.Key.Equals(requiredForeignKey.Key)
                            ? requiredForeignKey.Key
                            : null,
                        ChangedColumns = GetChangedColumns(currentForeignKey.Value.Columns, requiredForeignKey.Value.Columns, requiredTableColumns).ToArray(),
                        DescriptionChangedTo = currentForeignKey.Value.Description != requiredForeignKey.Value.Description
                            ? requiredForeignKey.Value.Description.AsChange()
                            : null
                    };

                    if (difference.IsChanged)
                        yield return difference;
                }
            }

            foreach (var currentForeignKey in currentForeignKeys.Where(col => !processedForeignKeys.Contains(col.Value)))
            {
                yield return new ForeignKeyDifference
                {
                    CurrentForeignKey = currentForeignKey.AsNamedItem(),
                    ForeignKeyDeleted = true
                };
            }
        }

        private IEnumerable<ForeignKeyColumnDifference> GetChangedColumns(
            IDictionary<string, string> currentColumns, 
            IDictionary<string, string> requiredColumns, 
            IDictionary<string, ColumnDetails> requiredTableColumns)
        {
            var requiredColumnToCurrentColumnMap = requiredColumns.ToDictionary(
                requiredColumn => requiredColumn.Key,
                requiredColumn =>
                {
                    var columnAlreadyIncludedWithSameName = currentColumns.GetNamedItem(requiredColumn.Key);
                    var columnAlreadyIncludedWithPreviousName = namedItemFinder.GetCurrentItem(
                        currentColumns,
                        requiredColumn.Key,
                        requiredTableColumns.ItemOrDefault(requiredColumn.Key)?.PreviousNames);

                    return columnAlreadyIncludedWithSameName ?? columnAlreadyIncludedWithPreviousName;
                });

            var processedCurrentColumnNames = new List<string>();

            foreach (var requiredColumn in requiredColumns)
            {
                var requiredColumnName = requiredColumn.Key;
                var requiredReferencedColumnName = requiredColumn.Value;
                var currentColumnMapping = requiredColumnToCurrentColumnMap.ItemOrDefault(requiredColumnName);

                //TODO: handle column rename in referenced table

                if (currentColumnMapping == null || !currentColumns.ContainsKey(currentColumnMapping.Key))
                {
                    yield return new ForeignKeyColumnDifference
                    {
                        ForeignKeyColumnAdded = true,
                        RequiredColumnName = requiredColumnName,
                        RequiredReferencedColumnName = requiredReferencedColumnName
                    };

                    continue;
                }

                processedCurrentColumnNames.Add(currentColumnMapping.Key);

                var difference = new ForeignKeyColumnDifference
                {
                    CurrentColumnName = currentColumnMapping.Key,
                    CurrentReferencedColumnName = currentColumnMapping.Value,
                    RequiredColumnName = requiredColumnName,
                    RequiredReferencedColumnName = requiredReferencedColumnName
                };

                if (difference.IsChanged)
                {
                    yield return difference;
                }
            }

            foreach (var currentColumnName in currentColumns.Keys.Except(processedCurrentColumnNames))
            {
                yield return new ForeignKeyColumnDifference
                {
                    ForeignKeyColumnDeleted = true,
                    CurrentColumnName = currentColumnName,
                    CurrentReferencedColumnName = currentColumns[currentColumnName]
                };
            }
        }
    }
}
