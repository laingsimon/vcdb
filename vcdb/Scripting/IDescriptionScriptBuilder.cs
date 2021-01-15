using vcdb.Output;

namespace vcdb.Scripting
{
    public interface IDescriptionScriptBuilder
    {
        SqlScript ChangeColumnDescription(TableName tableName, string requiredColumnName, string current, string required);
        SqlScript ChangeDatabaseDescription(string current, string required);
        SqlScript ChangeIndexDescription(TableName tableName, string requiredIndexName, string current, string required);
        SqlScript ChangeSchemaDescription(string requiredSchemaName, string current, string required);
        SqlScript ChangeTableDescription(TableName requiredTableName, string current, string required);
        SqlScript ChangePrimaryKeyDescription(TableName requiredTableName, string requiredKeyName, string current, string required);
    }
}