using System.Collections.Generic;
using vcdb.Output;
using vcdb.Scripting;
using vcdb.Scripting.ExecutionPlan;
using vcdb.Scripting.Permission;
using vcdb.Scripting.Programmability;

namespace vcdb.SqlServer.Scripting
{
    public class SqlServerProcedureScriptBuilder : IProcedureScriptBuilder
    {
        private readonly IPermissionScriptBuilder permissionScriptBuilder;
        private readonly IDescriptionScriptBuilder descriptionScriptBuilder;
        private readonly ISqlServerProgrammabilityHelper programmabilityHelper;

        public SqlServerProcedureScriptBuilder(
            IPermissionScriptBuilder permissionScriptBuilder,
            IDescriptionScriptBuilder descriptionScriptBuilder,
            ISqlServerProgrammabilityHelper programmabilityHelper)
        {
            this.permissionScriptBuilder = permissionScriptBuilder;
            this.descriptionScriptBuilder = descriptionScriptBuilder;
            this.programmabilityHelper = programmabilityHelper;
        }

        public IEnumerable<IScriptTask> CreateUpgradeScripts(IReadOnlyCollection<ProcedureDifference> procedureDifferences)
        {
            foreach (var procedureDifference in procedureDifferences)
            {
                var requiredProcedure = procedureDifference.RequiredProcedure;
                var currentProcedure = procedureDifference.CurrentProcedure;
                var changeRequiresProcedureRecreation = procedureDifference.SchemaBoundChangedTo != null || procedureDifference.EncryptedChangedTo != null;

                if (procedureDifference.ProcedureDeleted || changeRequiresProcedureRecreation)
                {
                    yield return GetDropProcedureScript(currentProcedure.Key);

                    if (!changeRequiresProcedureRecreation)
                    {
                        continue;
                    }
                }

                if (procedureDifference.ProcedureAdded || changeRequiresProcedureRecreation)
                {
                    yield return GetCreateProcedureScript(procedureDifference);
                }
                else
                {
                    if (procedureDifference.ProcedureRenamedTo != null)
                    {
                        yield return new MultiScriptTask(GetRenameProcedureScript(currentProcedure.Key, requiredProcedure.Key));
                    }

                    if (!changeRequiresProcedureRecreation && procedureDifference.DefinitionChangedTo != null)
                    {
                        yield return GetAlterProcedureScript(procedureDifference);
                    }
                }

                if (procedureDifference.DescriptionChangedTo != null 
                    || changeRequiresProcedureRecreation 
                    || (procedureDifference.ProcedureAdded && !string.IsNullOrEmpty(requiredProcedure.Value.Description)))
                {
                    yield return descriptionScriptBuilder.ChangeProcedureDescription(
                        requiredProcedure.Key,
                        currentProcedure?.Value?.Description,
                        procedureDifference.DescriptionChangedTo?.Value ?? requiredProcedure.Value.Description);
                }

                yield return new MultiScriptTask(permissionScriptBuilder.CreateProcedurePermissionScripts(
                    requiredProcedure.Key,
                    changeRequiresProcedureRecreation
                        ? PermissionDifferences.From(procedureDifference.RequiredProcedure.Value.Permissions)
                        : procedureDifference.PermissionDifferences));
            }
        }

        private IScriptTask GetAlterProcedureScript(ProcedureDifference procedureDifference)
        {
            var alterProcedureStatement = programmabilityHelper.ChangeProcedureInstructionTo(procedureDifference.DefinitionChangedTo, "ALTER");

            return new SqlScript(@$"{alterProcedureStatement.Trim()}
GO").CreatesOrAlters().Procedure(procedureDifference.RequiredProcedure.Key);
        }

        private IEnumerable<IScriptTask> GetRenameProcedureScript(ObjectName current, ObjectName required)
        {
            //TODO: Check if the schema has changed
            if (current.Name != required.Name)
            {
                yield return new SqlScript(@$"EXEC sp_rename
    @objname = '{current.Schema}.{current.Name}',
    @newname = '{required.Name}',
    @objtype = 'OBJECT'
GO").CreatesOrAlters().Procedure(required);
            }
        }

        private IScriptTask GetDropProcedureScript(ObjectName name)
        {
            return new SqlScript(@$"
DROP PROCEDURE {name.SqlSafeName()}
GO").Drops().Procedure(name);
        }

        private IScriptTask GetCreateProcedureScript(ProcedureDifference procedureDifference)
        {
            var createProcedureStatement = programmabilityHelper.ChangeProcedureInstructionTo(procedureDifference.DefinitionChangedTo, "CREATE");

            return new SqlScript($@"{createProcedureStatement.Trim()}
GO").CreatesOrAlters().Procedure(procedureDifference.RequiredProcedure.Key);
        }
    }
}
