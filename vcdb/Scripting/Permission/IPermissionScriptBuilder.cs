using System.Collections.Generic;
using vcdb.Output;

namespace vcdb.Scripting.Permission
{
    public interface IPermissionScriptBuilder
    {
        IEnumerable<SqlScript> CreateTablePermissionScripts(ObjectName tableName, PermissionDifferences permissionDifferences);
        IEnumerable<SqlScript> CreateSchemaPermissionScripts(string schemaName, PermissionDifferences permissionDifferences);
        IEnumerable<SqlScript> CreateColumnPermissionScripts(ObjectName tableName, string columnName, PermissionDifferences permissionDifferences);
        IEnumerable<SqlScript> CreateDatabasePermissionScripts(PermissionDifferences permissionDifferences);
    }
}
