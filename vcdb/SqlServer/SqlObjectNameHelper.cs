namespace vcdb.SqlServer
{
    public class SqlObjectNameHelper : ISqlObjectNameHelper
    {
        public string GetAutomaticConstraintName(string constraintPrefix, string tableName, string columnName, int objectId)
        {
            var objectIdAsHexadecimal = objectId.ToString("X");
            return $"{constraintPrefix}__{tableName}__{columnName}__{objectIdAsHexadecimal}";
        }
    }
}
