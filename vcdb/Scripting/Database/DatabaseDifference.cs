using System.Collections.Generic;
using vcdb.Scripting.Permission;
using vcdb.Scripting.Programmability;
using vcdb.Scripting.Schema;
using vcdb.Scripting.Table;
using vcdb.Scripting.User;

namespace vcdb.Scripting.Database
{
    public class DatabaseDifference
    {
        public IReadOnlyCollection<TableDifference> TableDifferences { get; set; }
        public IReadOnlyCollection<SchemaDifference> SchemaDifferences { get; set; }
        public IReadOnlyCollection<UserDifference> UserDifferences { get; set; }
        public IReadOnlyCollection<ProcedureDifference> ProcedureDifferences { get; set; }

        public Change<string> DescriptionChangedTo { get; set; }
        public string CollationChangedTo { get; set; }
        public PermissionDifferences PermissionDifferences { get; set; }
    }
}
