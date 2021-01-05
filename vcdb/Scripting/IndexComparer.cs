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
                        ChangedIncludedColumns = GetChangedColumns(currentIndex.Value.IncludedColumns, requiredIndex.Value.IncludedColumns, requiredTableColumns).ToArray(),
                        ClusteredChangedTo = requiredIndex.Value.Clustered != currentIndex.Value.Clustered 
                            ? requiredIndex.Value.Clustered
                            : default(bool?),
                        UniqueChangedTo = requiredIndex.Value.Unique != currentIndex.Value.Unique
                            ? requiredIndex.Value.Unique
                            : default(bool?)
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
            var removedColumns = currentColumns.Where(col => !requiredColumns.ContainsKey(col.Key));
            foreach (var removedColumn in removedColumns)
            {
                yield return new IndexColumnDetailsDifference
                {
                    CurrentColumn = removedColumn.AsNamedItem(),
                    ColumnRemoved = true
                };
            }

            var addedColumns = requiredColumns.Where(col => !currentColumns.ContainsKey(col.Key));

            foreach (var addedColumn in addedColumns)
            {
                yield return new IndexColumnDetailsDifference
                {
                    CurrentColumn = addedColumn.AsNamedItem(),
                    ColumnAdded = true
                };
            }

            foreach (var requiredColumn in requiredColumns)
            {
                var currentColumn = namedItemFinder.GetCurrentItem(
                    currentColumns,
                    requiredColumn.Key,
                    requiredTableColumns[requiredColumn.Key].PreviousNames);

                var difference = new IndexColumnDetailsDifference
                {
                    RequiredColumn = requiredColumn.AsNamedItem(),
                    CurrentColumn = currentColumn,
                    DescendingChangedTo = requiredColumn.Value.Descending != currentColumn.Value.Descending
                        ? requiredColumn.Value.Descending
                        : default(bool?)
                };

                if (difference.IsChanged)
                    yield return difference;
            }
        }
    }
}
