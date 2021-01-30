using System;

namespace vcdb.Scripting.Programmability
{
    public class InvalidDefinitionException : Exception
    {
        public InvalidDefinitionException(string message)
            :base(message)
        { }
    }
}
