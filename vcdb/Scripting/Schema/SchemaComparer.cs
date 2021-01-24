using System.Collections.Generic;
using System.Linq;
using vcdb.Models;
using vcdb.Scripting.Permission;

namespace vcdb.Scripting.Schema
{
    public class SchemaComparer : ISchemaComparer
    {
        private readonly IPermissionComparer permissionComparer;

        public SchemaComparer(IPermissionComparer permissionComparer)
        {
            this.permissionComparer = permissionComparer;
        }

        public IEnumerable<SchemaDifference> GetSchemaDifferences(
            ComparerContext context,
            IDictionary<string, SchemaDetails> currentSchemas,
            IDictionary<string, SchemaDetails> requiredSchemas)
        {
            var processedSchemas = new HashSet<SchemaDetails>();

            foreach (var requiredSchema in requiredSchemas)
            {
                var currentSchema = GetCurrentSchema(currentSchemas, requiredSchema);

                if (currentSchema == null)
                {
                    yield return new SchemaDifference
                    {
                        RequiredSchema = requiredSchema.AsNamedItem(),
                        SchemaAdded = true
                    };
                }
                else
                {
                    processedSchemas.Add(currentSchema.Value);

                    var differences = new SchemaDifference
                    {
                        RequiredSchema = requiredSchema.AsNamedItem(),
                        CurrentSchema = currentSchema,
                        SchemaRenamedTo = requiredSchema.Key != currentSchema.Key
                            ? requiredSchema.Key
                            : null,
                        DescriptionChangedTo = currentSchema.Value.Description != requiredSchema.Value.Description
                            ? requiredSchema.Value.Description.AsChange()
                            : null,
                        PermissionDifferences = permissionComparer.GetPermissionDifferences(
                            context,
                            currentSchema.Value.Permissions,
                            requiredSchema.Value.Permissions)
                    };

                    if (differences.IsChanged)
                        yield return differences;
                }
            }

            foreach (var currentSchema in currentSchemas.Where(schema => !processedSchemas.Contains(schema.Value)))
            {
                yield return new SchemaDifference
                {
                    CurrentSchema = currentSchema.AsNamedItem(),
                    SchemaDeleted = true
                };
            }
        }

        private NamedItem<string, SchemaDetails> GetCurrentSchema(
            IDictionary<string, SchemaDetails> currentSchemas,
            KeyValuePair<string, SchemaDetails> requiredSchema)
        {
            return GetCurrentSchema(currentSchemas, requiredSchema.Key)
                ?? GetCurrentSchemaForPreviousName(currentSchemas, requiredSchema.Value.PreviousNames);
        }

        private NamedItem<string, SchemaDetails> GetCurrentSchema(
            IDictionary<string, SchemaDetails> currentSchemas,
            string requiredSchemaName)
        {
            var tablesWithSameName = currentSchemas.Where(pair => pair.Key.Equals(requiredSchemaName)).ToArray();
            return tablesWithSameName.Length == 1
                ? tablesWithSameName[0].AsNamedItem()
                : NamedItem<string, SchemaDetails>.Null;
        }

        private NamedItem<string, SchemaDetails> GetCurrentSchemaForPreviousName(
            IDictionary<string, SchemaDetails> currentSchemas,
            string[] previousNames)
        {
            if (previousNames == null)
                return null;

            return previousNames
                .Select(previousName => GetCurrentSchema(currentSchemas, previousName))
                .FirstOrDefault(current => current != null);
        }
    }
}
