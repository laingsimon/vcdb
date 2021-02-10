using vcdb.Output;
using vcdb.Scripting;
using vcdb.Scripting.ExecutionPlan;

namespace vcdb.SqlServer.Scripting
{
    public class SqlServerDescriptionScriptBuilder : IDescriptionScriptBuilder
    {
        public IScriptTask ChangeDatabaseDescription(string current, string required)
        {
            return GetChangeDescriptionScript(null, null, null, null, current, required)
                .AsTask();
        }

        public IScriptTask ChangeSchemaDescription(string requiredSchemaName, string current, string required)
        {
            return GetChangeDescriptionScript(requiredSchemaName, null, null, null, current, required)
                .Requiring().Schema(requiredSchemaName).ToBeCreatedOrAltered();
        }

        public IScriptTask ChangeTableDescription(ObjectName requiredTableName, string current, string required)
        {
            return GetChangeDescriptionScript(requiredTableName.Schema, requiredTableName.Name, null, null, current, required)
                .Requiring().Table(requiredTableName).ToBeCreatedOrAltered();
        }

        public IScriptTask ChangeColumnDescription(ObjectName requiredTableName, string requiredColumnName, string current, string required)
        {
            return GetChangeDescriptionScript(requiredTableName.Schema, requiredTableName.Name, "COLUMN", requiredColumnName, current, required)
                .Requiring().Columns(requiredTableName.Component(requiredColumnName)).ToBeCreatedOrAltered();
        }

        public IScriptTask ChangeIndexDescription(ObjectName requiredTableName, string requiredIndexName, string current, string required)
        {
            return GetChangeDescriptionScript(requiredTableName.Schema, requiredTableName.Name, "INDEX", requiredIndexName, current, required)
                .Requiring().Index(requiredTableName.Component(requiredIndexName)).ToBeCreatedOrAltered();
        }

        public IScriptTask ChangePrimaryKeyDescription(ObjectName requiredTableName, string requiredKeyName, string current, string required)
        {
            return GetChangeDescriptionScript(requiredTableName.Schema, requiredTableName.Name, "CONSTRAINT", requiredKeyName, current, required)
                .Requiring().PrimaryKeyOn(requiredTableName).ToBeCreatedOrAltered();
        }

        public IScriptTask ChangeProcedureDescription(ObjectName requiredProcedureName, string current, string required)
        {
            return GetChangeDescriptionScript(requiredProcedureName.Schema, requiredProcedureName.Name, null, null, current, required, objectType: "PROCEDURE")
                .Requiring().Procedure(requiredProcedureName).ToBeCreatedOrAltered();
        }

        public IScriptTask ChangeForeignKeyDescription(ObjectName requiredTableName, string requiredForeignKeyName, string current, string required)
        {
            return GetChangeDescriptionScript(requiredTableName.Schema, requiredTableName.Name, "CONSTRAINT", requiredForeignKeyName, current, required)
                .Requiring().Table(requiredTableName).ToBeCreatedOrAltered();
        }

        private SqlScript GetChangeDescriptionScript(
            string schemaName,
            string objectName,
            string level2Type,
            string level2Name,
            string currentDescription,
            string requiredDescription,
            string objectType = "TABLE")
        {
            if (currentDescription == null && requiredDescription != null)
            {
                return new SqlScript($@"EXEC sp_addextendedproperty 
@name = N'MS_Description', @value = '{requiredDescription}',
@level0type = {GetQuotedValueOrNull(schemaName, "SCHEMA")}, @level0name = {GetQuotedValueOrNull(schemaName)}, 
@level1type = {GetQuotedValueOrNull(objectName, objectType)},  @level1name = {GetQuotedValueOrNull(objectName)},
@level2type = {GetQuotedValueOrNull(level2Type)}, @level2name = {GetQuotedValueOrNull(level2Name)}
GO");
            }

            if (currentDescription != null && requiredDescription == null)
            {
                return new SqlScript($@"EXEC sp_dropextendedproperty 
@name = N'MS_Description',
@level0type = {GetQuotedValueOrNull(schemaName, "SCHEMA")}, @level0name = {GetQuotedValueOrNull(schemaName)}, 
@level1type = {GetQuotedValueOrNull(objectName, objectType)},  @level1name = {GetQuotedValueOrNull(objectName)},
@level2type = {GetQuotedValueOrNull(level2Type)}, @level2name = {GetQuotedValueOrNull(level2Name)}
GO");
            }

            return new SqlScript($@"EXEC sp_updateextendedproperty 
@name = N'MS_Description', @value = '{requiredDescription}',
@level0type = {GetQuotedValueOrNull(schemaName, "SCHEMA")}, @level0name = {GetQuotedValueOrNull(schemaName)}, 
@level1type = {GetQuotedValueOrNull(objectName, objectType)},  @level1name = {GetQuotedValueOrNull(objectName)},
@level2type = {GetQuotedValueOrNull(level2Type)}, @level2name = {GetQuotedValueOrNull(level2Name)}
GO");
        }

        private static string GetQuotedValueOrNull(string value, string valueOverride = null)
        {
            return value == null
                ? "null"
                : $"N'{valueOverride ?? value}'";
        }
    }
}
