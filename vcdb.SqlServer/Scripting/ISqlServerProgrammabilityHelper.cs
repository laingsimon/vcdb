namespace vcdb.SqlServer.Scripting
{
    public interface ISqlServerProgrammabilityHelper
    {
        string ChangeProcedureInstructionTo(string definition, string requiredInstruction);
    }
}