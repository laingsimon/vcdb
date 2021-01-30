using vcdb.Models;
using vcdb.Scripting.Permission;

namespace vcdb.Scripting.Programmability
{
    public class ProcedureDifference
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
        public object DefinitionChangedTo { get; set; }

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
                    || DescriptionChangedTo != null;
            }
        }
    }
}
