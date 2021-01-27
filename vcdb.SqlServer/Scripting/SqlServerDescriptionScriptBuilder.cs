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

        public SqlScript ChangeTableDescription(TableName requiredTableName, string current, string required)
        {
            return GetChangeDescriptionScript(requiredTableName.Schema, requiredTableName.Table, null, null, current, required);
        }

        public SqlScript ChangeColumnDescription(TableName requiredTableName, string requiredColumnName, string current, string required)
        {
            return GetChangeDescriptionScript(requiredTableName.Schema, requiredTableName.Table, "COLUMN", requiredColumnName, current, required);
        }

        public SqlScript ChangeIndexDescription(TableName requiredTableName, string requiredIndexName, string current, string required)
        {
            return GetChangeDescriptionScript(requiredTableName.Schema, requiredTableName.Table, "INDEX", requiredIndexName, current, required);
        }

        public SqlScript ChangePrimaryKeyDescription(TableName requiredTableName, string requiredKeyName, string current, string required)
        {
            return GetChangeDescriptionScript(requiredTableName.Schema, requiredTableName.Table, "CONSTRAINT", requiredKeyName, current, required);
        }

        private SqlScript GetChangeDescriptionScript(
            string schemaName,
            string tableName,
            string level2Type,
            string level2Name,
            string currentDescription,
            string requiredDescription)
        {
            if (currentDescription == null && requiredDescription != null)
            {
                return new SqlScript($@"EXEC sp_addextendedproperty 
@name = N'MS_Description', @value = '{requiredDescription}',
@level0type = {GetQuotedValueOrNull(schemaName, "SCHEMA")}, @level0name = {GetQuotedValueOrNull(schemaName)}, 
@level1type = {GetQuotedValueOrNull(tableName, "TABLE")},  @level1name = {GetQuotedValueOrNull(tableName)},
@level2type = {GetQuotedValueOrNull(level2Type)}, @level2name = {GetQuotedValueOrNull(level2Name)}
GO");
            }

            if (currentDescription != null && requiredDescription == null)
            {
                return new SqlScript($@"EXEC sp_dropextendedproperty 
@name = N'MS_Description',
@level0type = {GetQuotedValueOrNull(schemaName, "SCHEMA")}, @level0name = {GetQuotedValueOrNull(schemaName)}, 
@level1type = {GetQuotedValueOrNull(tableName, "TABLE")},  @level1name = {GetQuotedValueOrNull(tableName)},
@level2type = {GetQuotedValueOrNull(level2Type)}, @level2name = {GetQuotedValueOrNull(level2Name)}
GO");
            }

            return new SqlScript($@"EXEC sp_updateextendedproperty 
@name = N'MS_Description', @value = '{requiredDescription}',
@level0type = {GetQuotedValueOrNull(schemaName, "SCHEMA")}, @level0name = {GetQuotedValueOrNull(schemaName)}, 
@level1type = {GetQuotedValueOrNull(tableName, "TABLE")},  @level1name = {GetQuotedValueOrNull(tableName)},
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
