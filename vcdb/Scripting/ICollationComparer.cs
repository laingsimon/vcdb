using vcdb.Models;

namespace vcdb.Scripting
{
    public interface ICollationComparer
    {
        string GetDatabaseCollationChange(
            DatabaseDetails currentDatabase,
            DatabaseDetails requiredDatabase);

        string GetColumnCollationChange(
            ComparerContext context,
            ColumnDetails currentColumn,
            ColumnDetails requiredRequired);
    }
}
