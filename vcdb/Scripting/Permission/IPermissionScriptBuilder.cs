using System.Collections.Generic;
using vcdb.Output;

namespace vcdb.Scripting.Permission
{
    public interface IPermissionScriptBuilder
    {
        IEnumerable<IOutputable> CreateTablePermissionScripts(ObjectName tableName, PermissionDifferences permissionDifferences);
        IEnumerable<IOutputable> CreateSchemaPermissionScripts(string schemaName, PermissionDifferences permissionDifferences);
        IEnumerable<IOutputable> CreateColumnPermissionScripts(ObjectName tableName, string columnName, PermissionDifferences permissionDifferences);
        IEnumerable<IOutputable> CreateDatabasePermissionScripts(PermissionDifferences permissionDifferences);
        IEnumerable<IOutputable> CreateProcedurePermissionScripts(ObjectName procedureName, PermissionDifferences permissionDifferences);
    }
}
