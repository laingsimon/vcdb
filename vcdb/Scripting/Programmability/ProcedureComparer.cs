using System.Collections.Generic;
using System.Linq;
using vcdb.CommandLine;
using vcdb.Models;
using vcdb.Scripting.Permission;

namespace vcdb.Scripting.Programmability
{
    public class ProcedureComparer : IProcedureComparer
    {
        private readonly INamedItemFinder namedItemFinder;
        private readonly IPermissionComparer permissionComparer;
        private readonly IInput input;

        public ProcedureComparer(
            INamedItemFinder namedItemFinder,
            IPermissionComparer permissionComparer,
            IInput input)
        {
            this.namedItemFinder = namedItemFinder;
            this.permissionComparer = permissionComparer;
            this.input = input;
        }

        public IEnumerable<ProcedureDifference> GetProcedureDifferences(
            ComparerContext context,
            IDictionary<ObjectName, ProcedureDetails> currentProcedures,
            IDictionary<ObjectName, ProcedureDetails> requiredProcedures)
        {
            var processedProcedures = new HashSet<ProcedureDetails>();
            foreach (var requiredProcedure in requiredProcedures)
            {
                var currentProcedure = namedItemFinder.GetCurrentItem(currentProcedures, requiredProcedure);

                if (currentProcedure == null)
                {
                    yield return new ProcedureDifference
                    {
                        RequiredProcedure = requiredProcedure.AsNamedItem(),
                        ProcedureAdded = true
                    };
                }
                else
                {
                    processedProcedures.Add(currentProcedure.Value);

                    var difference = new ProcedureDifference
                    {
                        CurrentProcedure = currentProcedure,
                        RequiredProcedure = requiredProcedure.AsNamedItem(),
                        EncryptedChangedTo = currentProcedure.Value.Encrypted != requiredProcedure.Value.Encrypted
                            ? requiredProcedure.Value.Encrypted.AsChange()
                            : null,
                        SchemaBoundChangedTo = currentProcedure.Value.SchemaBound != requiredProcedure.Value.SchemaBound
                            ? requiredProcedure.Value.SchemaBound.AsChange()
                            : null,
                        DefinitionChangedTo = DefinitionHasChanged(currentProcedure.Value.Definition, GetDefinition(requiredProcedure.Value))
                            ? GetDefinition(requiredProcedure.Value)
                            : null,
                        ProcedureRenamedTo = !currentProcedure.Key.Equals(requiredProcedure.Key)
                            ? requiredProcedure.Key
                            : null,
                        DescriptionChangedTo = currentProcedure.Value.Description != requiredProcedure.Value.Description
                            ? requiredProcedure.Value.Description.AsChange()
                            : null,
                        PermissionDifferences = permissionComparer.GetPermissionDifferences(
                            context,
                            currentProcedure.Value.Permissions,
                            requiredProcedure.Value.Permissions)
                    };

                    if (difference.IsChanged)
                    {
                        yield return difference;
                    }
                }
            }

            foreach (var currentProcedure in currentProcedures.Where(col => !processedProcedures.Contains(col.Value)))
            {
                yield return new ProcedureDifference
                {
                    CurrentProcedure = currentProcedure.AsNamedItem(),
                    ProcedureDeleted = true
                };
            }
        }

        private bool DefinitionHasChanged(string currentDefinition, string requiredDefinition)
        {
            return currentDefinition != requiredDefinition;
        }

        private string GetDefinition(ProcedureDetails procedure)
        {
            if (!string.IsNullOrEmpty(procedure.Definition))
            {
                return procedure.Definition;
            }

            return input.GetSiblingContent(procedure.FileDefinition)
                .ReadToEnd();
        }
    }
}
