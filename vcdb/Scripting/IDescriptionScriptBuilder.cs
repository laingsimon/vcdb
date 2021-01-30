using vcdb.Output;

namespace vcdb.Scripting
{
    public interface IDescriptionScriptBuilder
    {
        SqlScript ChangeColumnDescription(ObjectName tableName, string requiredColumnName, string current, string required);
        SqlScript ChangeDatabaseDescription(string current, string required);
        SqlScript ChangeIndexDescription(ObjectName tableName, string requiredIndexName, string current, string required);
        SqlScript ChangeSchemaDescription(string requiredSchemaName, string current, string required);
        SqlScript ChangeTableDescription(ObjectName requiredTableName, string current, string required);
        SqlScript ChangePrimaryKeyDescription(ObjectName requiredTableName, string requiredKeyName, string current, string required);
    }
}