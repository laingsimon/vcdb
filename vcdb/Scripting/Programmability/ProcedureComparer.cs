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
        private readonly IProcedureDefinitionValidator definitionValidator;
        private readonly IProcedureDefinitionComparer procedureDefinitionComparer;

        public ProcedureComparer(
            INamedItemFinder namedItemFinder,
            IPermissionComparer permissionComparer,
            IInput input,
            IProcedureDefinitionValidator definitionValidator,
            IProcedureDefinitionComparer procedureDefinitionComparer)
        {
            this.namedItemFinder = namedItemFinder;
            this.permissionComparer = permissionComparer;
            this.input = input;
            this.definitionValidator = definitionValidator;
            this.procedureDefinitionComparer = procedureDefinitionComparer;
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
                        DefinitionChangedTo = GetDefinition(requiredProcedure.AsNamedItem(), null),
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
                        DefinitionChangedTo = GetDefinitionChangedTo(currentProcedure, requiredProcedure.AsNamedItem()),
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

        private string GetDefinitionChangedTo(
            NamedItem<ObjectName, ProcedureDetails> currentProcedure,
            NamedItem<ObjectName, ProcedureDetails> requiredProcedure)
        {
            var requiredDefinition = GetDefinition(requiredProcedure, currentProcedure.Key);
            var definitionChanged = !procedureDefinitionComparer.Equals(
                new NamedItem<ObjectName, string>(currentProcedure.Key, currentProcedure.Value.Definition),
                new NamedItem<ObjectName, string>(requiredProcedure.Key, requiredDefinition));

            return definitionChanged
                ? requiredDefinition
                : null;
        }

        private string GetDefinition(NamedItem<ObjectName, ProcedureDetails> procedure, ObjectName otherAllowedName)
        {
            var definition = string.IsNullOrEmpty(procedure.Value.Definition) && !string.IsNullOrEmpty(procedure.Value.FileDefinition)
                ? input.GetSiblingContent(procedure.Value.FileDefinition).ReadToEnd()
                : procedure.Value.Definition;

            if (string.IsNullOrEmpty(definition))
                throw new InvalidDefinitionException($"Definition could not be read/found for {procedure.Key.Schema}.{procedure.Key.Name}");

            var validationErrors = definitionValidator.ValidateDefinition(definition, procedure, otherAllowedName).ToArray();
            if (validationErrors.Any())
            {
                throw new InvalidDefinitionException(@$"Procedure definition for {procedure.Key.Schema}.{procedure.Key.Name} is invalid
{string.Join(",", validationErrors)}");
            }

            return definition;
        }
    }
}
