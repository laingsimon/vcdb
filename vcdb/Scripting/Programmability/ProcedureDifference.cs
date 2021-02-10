using vcdb.Models;
using vcdb.Scripting.ExecutionPlan;
using vcdb.Scripting.Permission;
using vcdb.Scripting.Schema;

namespace vcdb.Scripting.Programmability
{
    public class ProcedureDifference : ISchemaObjectDifference
    {
        public NamedItem<ObjectName, ProcedureDetails> RequiredProcedure { get; set; }
        public NamedItem<ObjectName, ProcedureDetails> CurrentProcedure { get; set; }
        public bool ProcedureAdded { get; set; }
        public bool ProcedureDeleted { get; set; }

        public ObjectName ProcedureRenamedTo { get; set; }
        public Change<string> DescriptionChangedTo { get; set; }
        public PermissionDifferences PermissionDifferences { get; set; }
        public Change<bool> EncryptedChangedTo { get; set; }
        public Change<bool> SchemaBoundChangedTo { get; set; }
        public string DefinitionChangedTo { get; set; }

        public bool IsChanged
        {
            get
            {
                return ProcedureAdded
                    || ProcedureDeleted
                    || ProcedureRenamedTo != null
                    || PermissionDifferences != null
                    || EncryptedChangedTo != null
                    || SchemaBoundChangedTo != null
                    || DescriptionChangedTo != null
                    || DefinitionChangedTo != null;
            }
        }

        bool ISchemaObjectDifference.ObjectAdded => ProcedureAdded;
        bool ISchemaObjectDifference.ObjectDeleted => ProcedureDeleted;
        ObjectName ISchemaObjectDifference.ObjectRenamedTo => ProcedureRenamedTo;
        ObjectName ISchemaObjectDifference.CurrentName => CurrentProcedure?.Key;
        ObjectName ISchemaObjectDifference.RequiredName => RequiredProcedure?.Key;

        IScriptTask ISchemaObjectDifference.GetScriptTask(IScriptTask script)
        {
            if (ProcedureDeleted)
                return script.Drops().Procedure(CurrentProcedure.Key);

            return script.CreatesOrAlters().Procedure(RequiredProcedure.Key);
        }
    }
}
