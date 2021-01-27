namespace vcdb.SqlServer
{
    public class SqlObjectNameHelper : ISqlObjectNameHelper
    {
        public string GetAutomaticConstraintName(string constraintPrefix, string tableName, string columnName, int objectId)
        {
            var columnNamePart = string.IsNullOrEmpty(columnName)
                ? ""
                : "__" + columnName;
            var objectIdSuffix = objectId == 0
                ? ""
                : "__" + objectId.ToString("X");

            return $"{constraintPrefix}__{tableName}{columnNamePart}{objectIdSuffix}";
        }

        public string GetAutomaticConstraintName(string constraintPrefix, string tableName, string columnName, string objectIdSubstitute)
        {
            var columnNamePart = string.IsNullOrEmpty(columnName)
                ? ""
                : "__" + columnName;
            var objectIdPart = string.IsNullOrEmpty(objectIdSubstitute)
                ? ""
                : "__" + objectIdSubstitute;
            return $"{constraintPrefix}__{tableName}{columnNamePart}{objectIdPart}";
        }
    }
}
