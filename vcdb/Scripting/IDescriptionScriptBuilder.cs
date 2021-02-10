using vcdb.Scripting.ExecutionPlan;

namespace vcdb.Scripting
{
    public interface IDescriptionScriptBuilder
    {
        IScriptTask ChangeColumnDescription(ObjectName tableName, string requiredColumnName, string current, string required);
        IScriptTask ChangeDatabaseDescription(string current, string required);
        IScriptTask ChangeIndexDescription(ObjectName tableName, string requiredIndexName, string current, string required);
        IScriptTask ChangeSchemaDescription(string requiredSchemaName, string current, string required);
        IScriptTask ChangeTableDescription(ObjectName requiredTableName, string current, string required);
        IScriptTask ChangePrimaryKeyDescription(ObjectName requiredTableName, string requiredKeyName, string current, string required);
        IScriptTask ChangeProcedureDescription(ObjectName procedureName, string current, string required);
        IScriptTask ChangeForeignKeyDescription(ObjectName requiredTableName, string requiredPrimaryKeyName, string current, string required);
    }
}