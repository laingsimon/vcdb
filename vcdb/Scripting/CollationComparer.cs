using vcdb.Models;

namespace vcdb.Scripting
{
    public class CollationComparer : ICollationComparer
    {
        public string GetColumnCollationChange(ComparerContext context, ColumnDetails currentColumn, ColumnDetails requiredColumn)
        {
            if (!IsCollationApplicable(requiredColumn.Type))
                return null; //collation doesnt apply to this column type

            var resolvedCurrentDatabaseCollation = context.CurrentDatabase.Collation ?? context.CurrentDatabase.ServerCollation;
            var resolvedCurrentColumnCollation = currentColumn.Collation ?? resolvedCurrentDatabaseCollation;
            var resolvedRequiredColumnCollation = requiredColumn.Collation ?? context.RequiredDatabase.Collation ?? context.CurrentDatabase.ServerCollation;

            if (resolvedCurrentColumnCollation == resolvedRequiredColumnCollation)
                return null; //no change

            return resolvedRequiredColumnCollation; //the column collation needs to change towards or away from the default database collation
        }

        private bool IsCollationApplicable(string type)
        {
            var normalisedType = type.Contains("(")
                ? type.Substring(0, type.IndexOf("("))
                : type;

            switch (normalisedType.ToLower())
            {
                case "varchar":
                case "nvarchar":
                case "char":
                case "nchar":
                case "text":
                case "ntext":
                    return true;
                default:
                    return false;
            }
        }

        public string GetDatabaseCollationChange(
            DatabaseDetails currentDatabase,
            DatabaseDetails requiredDatabase)
        {
            if (currentDatabase.Collation == requiredDatabase.Collation)
                return null;

            if (currentDatabase.Collation == null)
            {
                if (requiredDatabase.Collation == currentDatabase.ServerCollation)
                    return null; //user has expressed the database to use the same as the server collation

                return requiredDatabase.Collation; //user has specified a different collation to the server collation. The database is using the server collation currently
            }

            return requiredDatabase.Collation ?? currentDatabase.ServerCollation; //the database collation needs to change, it might be changing to the server default collation or something else
        }
    }
}
