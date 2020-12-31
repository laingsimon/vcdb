using System.Collections.Generic;
using vcdb.Models;

namespace vcdb.Scripting
{
    public interface IColumnComparer
    {
        IEnumerable<ColumnDifference> GetDifferentColumns(
            Dictionary<string, ColumnDetails> currentColumns, 
            Dictionary<string, ColumnDetails> requiredColumns);
    }
}