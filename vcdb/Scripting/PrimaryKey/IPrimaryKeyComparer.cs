using vcdb.Models;

namespace vcdb.Scripting.PrimaryKey
{
    public interface IPrimaryKeyComparer
    {
        PrimaryKeyDifference GetPrimaryKeyDifference(ComparerContext context, TableDetails currentTable, TableDetails requiredTable);
    }
}