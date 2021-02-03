using System.Collections.Generic;

namespace vcdb.Scripting.Programmability
{
    public interface IProcedureDefinitionComparer : IEqualityComparer<NamedItem<ObjectName, string>>
    {
    }
}
