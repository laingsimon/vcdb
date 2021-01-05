using System.Collections.Generic;
using vcdb.Models;

namespace vcdb.Scripting
{
    public interface IColumnComparer
    {
        IEnumerable<ColumnDifference> GetDifferentColumns(
            IDictionary<string, ColumnDetails> currentColumns, 
            IDictionary<string, ColumnDetails> requiredColumns);
    }
}