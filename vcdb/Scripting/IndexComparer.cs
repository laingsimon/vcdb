using System.Collections.Generic;
using System.Linq;
using vcdb.Models;

namespace vcdb.Scripting
{
    public class IndexComparer : IIndexComparer
    {
        private readonly INamedItemFinder namedItemFinder;

        public IndexComparer(INamedItemFinder namedItemFinder)
        {
            this.namedItemFinder = namedItemFinder;
        }

        public IEnumerable<IndexDifference> GetIndexDifferences(
            IDictionary<string, IndexDetails> currentIndexes,
            IDictionary<string, IndexDetails> requiredIndexes,
            IDictionary<string, ColumnDetails> requiredTableColumns)
        {
            var processedIndexes = new HashSet<IndexDetails>();
            foreach (var requiredIndex in requiredIndexes)
            {
                var currentIndex = namedItemFinder.GetCurrentItem(currentIndexes, requiredIndex);

                if (currentIndex == null)
                {
                    yield return new IndexDifference
                    {
                        RequiredIndex = requiredIndex.AsNamedItem(),
                        IndexAdded = true
                    };
                }
                else
                {
                    processedIndexes.Add(currentIndex.Value);

                    var difference = new IndexDifference
                    {
                        CurrentIndex = currentIndex,
                        RequiredIndex = requiredIndex.AsNamedItem(),
                        IndexRenamedTo = !currentIndex.Key.Equals(requiredIndex.Key)
                            ? requiredIndex.Key
                            : null,
                        ChangedColumns = GetChangedColumns(currentIndex.Value.Columns, requiredIndex.Value.Columns, requiredTableColumns).ToArray(),
                        ChangedIncludedColumns = GetChangedColumns(currentIndex.Value.Including, requiredIndex.Value.Including, requiredTableColumns).ToArray(),
                        ClusteredChangedTo = requiredIndex.Value.Clustered != currentIndex.Value.Clustered 
                            ? requiredIndex.Value.Clustered
                            : default(bool?),
                        UniqueChangedTo = requiredIndex.Value.Unique != currentIndex.Value.Unique
                            ? requiredIndex.Value.Unique
                            : default(bool?),
                        DescriptionChangedTo = currentIndex.Value.Description != requiredIndex.Value.Description
                            ? requiredIndex.Value.Description.AsChange()
                            : null
                    };

                    if (difference.IsChanged)
                        yield return difference;
                }
            }

            foreach (var currentIndex in currentIndexes.Where(col => !processedIndexes.Contains(col.Value)))
            {
                yield return new IndexDifference
                {
                    CurrentIndex = currentIndex.AsNamedItem(),
                    IndexDeleted = true
                };
            }
        }

        private IEnumerable<IndexColumnDetailsDifference> GetChangedColumns(
            IDictionary<string, IndexColumnDetails> currentColumns,
            IDictionary<string, IndexColumnDetails> requiredColumns,
            IDictionary<string, ColumnDetails> requiredTableColumns)
        {
            var requiredColumnToCurrentColumnMap = requiredColumns.ToDictionary(
                requiredColumn => requiredColumn.Key,
                requiredColumn => {
                    var columnAlreadyIncludedWithSameName = currentColumns.GetNamedItem(requiredColumn.Key);
                    var columnAlreadyIncludedWithPreviousName = namedItemFinder.GetCurrentItem(
                        currentColumns,
                        requiredColumn.Key,
                        requiredTableColumns.ItemOrDefault(requiredColumn.Key)?.PreviousNames);

                    return columnAlreadyIncludedWithSameName ?? columnAlreadyIncludedWithPreviousName;
                });

            foreach (var addedColumnMap in requiredColumnToCurrentColumnMap.Where(map => map.Value == null))
            {
                yield return new IndexColumnDetailsDifference
                {
                    CurrentColumn = addedColumnMap.Value,
                    ColumnAdded = true
                };
            }

            var removedColumns = currentColumns
                .Where(currentColumn => !requiredColumnToCurrentColumnMap.Values
                    .Where(mappedCurrentColumn => mappedCurrentColumn != null)
                    .Any(mappedCurrentColumn => mappedCurrentColumn.Key == currentColumn.Key));
            foreach (var removedColumn in removedColumns)
            {
                yield return new IndexColumnDetailsDifference
                {
                    CurrentColumn = removedColumn.AsNamedItem(),
                    ColumnRemoved = true
                };
            }
            
            foreach (var columnMapping in requiredColumnToCurrentColumnMap.Where(columnMapping => columnMapping.Value != null))
            {
                var currentColumn = columnMapping.Value;
                var requiredColumn = requiredColumns.GetNamedItem(columnMapping.Key);

                var difference = new IndexColumnDetailsDifference
                {
                    RequiredColumn = requiredColumn,
                    CurrentColumn = currentColumn,
                    DescendingChangedTo = requiredColumn.Value.Descending != currentColumn.Value.Descending
                        ? requiredColumn.Value.Descending
                        : default(bool?)
                };

                if (difference.IsChanged)
                    yield return difference;
            }
        }

        private IEnumerable<IndexColumnDetailsDifference> GetChangedColumns(
            IReadOnlyCollection<string> currentColumns,
            IReadOnlyCollection<string> requiredColumns,
            IDictionary<string, ColumnDetails> requiredTableColumns)
        {
            var currentColumnProxy = currentColumns.ToDictionary(columnName => columnName);

            var requiredColumnToCurrentColumnMap = (requiredColumns ?? new string[0]).ToDictionary(
                requiredColumn => requiredColumn,
                requiredColumn => {
                    var columnAlreadyIncludedWithSameName = currentColumns.SingleOrDefault(col => col == requiredColumn);
                    var columnAlreadyIncludedWithPreviousName = namedItemFinder.GetCurrentItem(
                        currentColumnProxy,
                        requiredColumn,
                        requiredTableColumns.ItemOrDefault(requiredColumn)?.PreviousNames);

                    return columnAlreadyIncludedWithSameName ?? columnAlreadyIncludedWithPreviousName?.Key;
                });

            foreach (var addedColumn in requiredColumnToCurrentColumnMap.Where(map => map.Value == null))
            {
                yield return new IndexColumnDetailsDifference
                {
                    CurrentColumn = new NamedItem<string, IndexColumnDetails>(addedColumn.Key, null),
                    ColumnAdded = true
                };
            }

            var removedColumns = currentColumns
                .Where(currentColumn => !requiredColumnToCurrentColumnMap.Values
                    .Where(mappedCurrentColumn => mappedCurrentColumn != null)
                    .Any(mappedCurrentColumn => mappedCurrentColumn == currentColumn));
            foreach (var removedColumn in removedColumns)
            {
                yield return new IndexColumnDetailsDifference
                {
                    CurrentColumn = new NamedItem<string, IndexColumnDetails>(removedColumn, null),
                    ColumnRemoved = true
                };
            }
        }
    }
}
