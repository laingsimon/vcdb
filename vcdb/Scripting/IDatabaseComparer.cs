using vcdb.Models;

namespace vcdb.Scripting
{
    public interface IDatabaseComparer
    {
        DatabaseDifference GetDatabaseDifferences(
            ComparerContext context,
            DatabaseDetails currentDatabase, 
            DatabaseDetails requiredDatabase);
    }
}