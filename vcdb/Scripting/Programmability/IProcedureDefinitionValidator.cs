using System.Collections.Generic;
using vcdb.Models;

namespace vcdb.Scripting.Programmability
{
    public interface IProcedureDefinitionValidator
    {
        IEnumerable<string> ValidateDefinition(
            string definition,
            NamedItem<ObjectName, ProcedureDetails> procedure,
            ObjectName otherAllowedName);
    }
}
