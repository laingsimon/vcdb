using vcdb.Models;

namespace vcdb.Scripting
{
    public interface IDatabaseComparer
    {
        DatabaseDifference GetDatabaseDifferences(DatabaseDetails currentDatabase, DatabaseDetails requiredDatabase);
    }
}