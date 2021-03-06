﻿using System.Collections.Generic;
using vcdb.Models;

namespace vcdb.Scripting.Schema
{
    public interface ISchemaComparer
    {
        IEnumerable<SchemaDifference> GetSchemaDifferences(
            ComparerContext context,
            IDictionary<string, SchemaDetails> currentSchemas,
            IDictionary<string, SchemaDetails> requiredSchemas);
    }
}