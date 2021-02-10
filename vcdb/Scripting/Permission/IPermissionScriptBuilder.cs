using System.Collections.Generic;
using vcdb.Scripting.ExecutionPlan;

namespace vcdb.Scripting.Permission
{
    public interface IPermissionScriptBuilder
    {
        IEnumerable<IScriptTask> CreateTablePermissionScripts(ObjectName tableName, PermissionDifferences permissionDifferences);
        IEnumerable<IScriptTask> CreateSchemaPermissionScripts(string schemaName, PermissionDifferences permissionDifferences);
        IEnumerable<IScriptTask> CreateColumnPermissionScripts(ObjectName tableName, string columnName, PermissionDifferences permissionDifferences);
        IEnumerable<IScriptTask> CreateDatabasePermissionScripts(PermissionDifferences permissionDifferences);
        IEnumerable<IScriptTask> CreateProcedurePermissionScripts(ObjectName procedureName, PermissionDifferences permissionDifferences);
    }
}
