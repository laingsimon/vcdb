using System.Collections.Generic;
using System.Linq;
using vcdb.Models;
using vcdb.Scripting.CheckConstraint;
using vcdb.Scripting.Column;
using vcdb.Scripting.ForeignKey;
using vcdb.Scripting.Index;
using vcdb.Scripting.Permission;
using vcdb.Scripting.PrimaryKey;
using vcdb.Scripting.Schema;

namespace vcdb.Scripting.Table
{
    public class TableDifference : ISchemaObjectDifference
    {
        public NamedItem<ObjectName, TableDetails> CurrentTable { get; set; }
        public NamedItem<ObjectName, TableDetails> RequiredTable { get; set; }

        public ObjectName TableRenamedTo { get; set; }
        public bool TableAdded { get; set; }
        public bool TableDeleted { get; set; }
        public Change<string> DescriptionChangedTo { get; set; }

        public IReadOnlyCollection<ColumnDifference> ColumnDifferences { get; set; }
        public IReadOnlyCollection<IndexDifference> IndexDifferences { get; set; }
        public IReadOnlyCollection<CheckConstraintDifference> ChangedCheckConstraints { get; set; }
        public PrimaryKeyDifference PrimaryKeyDifference { get; set; }
        public PermissionDifferences PermissionDifferences { get; set; }
        public IReadOnlyCollection<ForeignKeyDifference> ForeignKeyDifferences { get; set; }

        public bool IsChanged
        {
            get
            {
                return TableAdded
                    || TableDeleted
                    || TableRenamedTo != null
                    || ColumnDifferences?.Any() == true
                    || IndexDifferences?.Any() == true
                    || DescriptionChangedTo != null
                    || ChangedCheckConstraints?.Any() == true
                    || PrimaryKeyDifference != null
                    || PermissionDifferences != null
                    || ForeignKeyDifferences?.Any() == true;
            }
        }

        bool ISchemaObjectDifference.ObjectAdded => TableAdded;
        bool ISchemaObjectDifference.ObjectDeleted => TableDeleted;
        ObjectName ISchemaObjectDifference.ObjectRenamedTo => TableRenamedTo;
        ObjectName ISchemaObjectDifference.CurrentName => CurrentTable?.Key;
        ObjectName ISchemaObjectDifference.RequiredName => RequiredTable?.Key;
    }
}
