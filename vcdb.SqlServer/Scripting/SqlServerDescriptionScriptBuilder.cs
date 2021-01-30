using vcdb.Output;
using vcdb.Scripting;

namespace vcdb.SqlServer.Scripting
{
    public class SqlServerDescriptionScriptBuilder : IDescriptionScriptBuilder
    {
        public SqlScript ChangeDatabaseDescription(string current, string required)
        {
            return GetChangeDescriptionScript(null, null, null, null, current, required);
        }

        public SqlScript ChangeSchemaDescription(string requiredSchemaName, string current, string required)
        {
            return GetChangeDescriptionScript(requiredSchemaName, null, null, null, current, required);
        }

        public SqlScript ChangeTableDescription(ObjectName requiredTableName, string current, string required)
        {
            return GetChangeDescriptionScript(requiredTableName.Schema, requiredTableName.Name, null, null, current, required);
        }

        public SqlScript ChangeColumnDescription(ObjectName requiredTableName, string requiredColumnName, string current, string required)
        {
            return GetChangeDescriptionScript(requiredTableName.Schema, requiredTableName.Name, "COLUMN", requiredColumnName, current, required);
        }

        public SqlScript ChangeIndexDescription(ObjectName requiredTableName, string requiredIndexName, string current, string required)
        {
            return GetChangeDescriptionScript(requiredTableName.Schema, requiredTableName.Name, "INDEX", requiredIndexName, current, required);
        }

        public SqlScript ChangePrimaryKeyDescription(ObjectName requiredTableName, string requiredKeyName, string current, string required)
        {
            return GetChangeDescriptionScript(requiredTableName.Schema, requiredTableName.Name, "CONSTRAINT", requiredKeyName, current, required);
        }

        public SqlScript ChangeProcedureDescription(ObjectName requiredProcedureName, string current, string required)
        {
            return GetChangeDescriptionScript(requiredProcedureName.Schema, requiredProcedureName.Name, null, null, current, required, objectType: "PROCEDURE");
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
