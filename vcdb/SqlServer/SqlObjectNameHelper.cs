namespace vcdb.SqlServer
{
    public class SqlObjectNameHelper : ISqlObjectNameHelper
    {
        public string GetAutomaticConstraintName(string constraintPrefix, string tableName, string columnName, int objectId)
        {
            var objectIdSuffix = objectId == 0
                ? ""
                : "__" + objectId.ToString("X");
            return $"{constraintPrefix}__{tableName}__{columnName}{objectIdSuffix}";
        }
    }
}
