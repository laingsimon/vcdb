using System.Collections.Generic;
using vcdb.Models;

namespace vcdb.Scripting
{
    public interface ISchemaComparer
    {
        IEnumerable<SchemaDifference> GetSchemaDifferences(IDictionary<string, SchemaDetails> currentSchemas, IDictionary<string, SchemaDetails> requiredSchemas);
    }
}