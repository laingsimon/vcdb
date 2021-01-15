using vcdb.Models;

namespace vcdb.Scripting.Database
{
    public interface IDatabaseComparer
    {
        DatabaseDifference GetDatabaseDifferences(
            ComparerContext context,
            DatabaseDetails currentDatabase,
            DatabaseDetails requiredDatabase);
    }
}