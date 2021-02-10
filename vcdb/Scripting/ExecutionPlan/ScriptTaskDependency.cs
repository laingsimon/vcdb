using System.Collections.Generic;
using System.Diagnostics;

namespace vcdb.Scripting.ExecutionPlan
{
    [DebuggerDisplay("{DebugString()}")]
    public class ScriptTaskDependency
    {
        public DependencyAction Action { get; }
        public TableComponentName Column { get; }
        public ObjectName Table { get; }
        public ObjectName PrimaryKeyOn { get; }
        public ObjectName ForeignKeyReferencing { get; }
        public TableComponentName Index { get; }
        public string Schema { get; }
        public string User { get; }
        public ObjectName Procedure { get; }
        public TableComponentName CheckConstraintOn { get; }
        public TableComponentName ForeignKeyOn { get; }

        public ScriptTaskDependency(
           DependencyAction action,
           TableComponentName column = null,
           ObjectName table = null,
           ObjectName primaryKeyOn = null,
           ObjectName foreignKeyReferencing = null,
           TableComponentName index  = null,
           string schema = null,
           string user = null,
           ObjectName procedure = null,
           TableComponentName checkConstraintOn = null,
           TableComponentName foreignKeyOn = null)
        {
            Action = action;
            Column = column;
            Table = table;
            PrimaryKeyOn = primaryKeyOn;
            ForeignKeyReferencing = foreignKeyReferencing;
            Index = index;
            Schema = schema;
            User = user;
            Procedure = procedure;
            CheckConstraintOn = checkConstraintOn;
            ForeignKeyOn = foreignKeyOn;
        }

        public bool Equals(ScriptTaskDependency other)
        {
            if (other == null)
            {
                return false;
            }

            return Action == other.Action
               && ((Column == null && other.Column == null) || Column?.Equals(other.Column) == true)
               && ((Table == null && other.Table == null) || Table?.Equals(other.Table) == true)
               && ((PrimaryKeyOn == null && other.PrimaryKeyOn == null) || PrimaryKeyOn?.Equals(other.PrimaryKeyOn) == true)
               && ((ForeignKeyReferencing == null) && other.ForeignKeyReferencing == null || ForeignKeyReferencing?.Equals(other.ForeignKeyReferencing) == true)
               && ((Index == null && other.Index == null) || Index?.Equals(other.Index) == true)
               && ((Schema == null && other.Schema == null) || Schema?.Equals(other.Schema) == true)
               && ((User == null && other.User == null) || User?.Equals(other.User) == true)
               && ((Procedure == null && other.Procedure == null) || Procedure?.Equals(other.Procedure) == true)
               && ((CheckConstraintOn == null && other.CheckConstraintOn == null) || CheckConstraintOn?.Equals(other.CheckConstraintOn) == true)
               && ((ForeignKeyOn == null && other.ForeignKeyOn == null) || ForeignKeyOn?.Equals(other.ForeignKeyOn) == true);
        }

        public override bool Equals(object other)
        {
            return Equals(other as ScriptTaskDependency);
        }

        public override int GetHashCode()
        {
            return Action.GetHashCode()
            ^ (Column?.GetHashCode() ?? 0)
            ^ (Table?.GetHashCode() ?? 0)
            ^ (PrimaryKeyOn?.GetHashCode() ?? 0)
            ^ (ForeignKeyReferencing?.GetHashCode() ?? 0)
            ^ (Index?.GetHashCode() ?? 0)
            ^ (Schema?.GetHashCode() ?? 0)
            ^ (User?.GetHashCode() ?? 0)
            ^ (Procedure?.GetHashCode() ?? 0)
            ^ (CheckConstraintOn?.GetHashCode() ?? 0)
            ^ (ForeignKeyOn?.GetHashCode() ?? 0);
        }

        internal string DebugString()
        {
            return $"{Action}: {string.Join(", ", GetDependencies())}";
        }

        private IEnumerable<string> GetDependencies()
        {
            if (Column != null)
                yield return $"{nameof(Column)}: {Column.DebugString()}";
            if (Table != null)
                yield return $"{nameof(Table)}: {Table.DebugString()}";
            if (PrimaryKeyOn != null)
                yield return $"{nameof(PrimaryKeyOn)}: {PrimaryKeyOn.DebugString()}";
            if (ForeignKeyReferencing != null)
                yield return $"{nameof(ForeignKeyReferencing)}: {ForeignKeyReferencing.DebugString()}";
            if (Index != null)
                yield return $"{nameof(Index)}: {Index.DebugString()}";
            if (Schema != null)
                yield return $"{nameof(Schema)}: {Schema}";
            if (User != null)
                yield return $"{nameof(User)}: {User}";
            if (Procedure != null)
                yield return $"{nameof(Procedure)}: {Procedure.DebugString()}";
            if (CheckConstraintOn != null)
                yield return $"{nameof(CheckConstraintOn)}: {CheckConstraintOn.DebugString()}";
            if (ForeignKeyOn != null)
                yield return $"{nameof(ForeignKeyOn)}: {ForeignKeyOn.DebugString()}";
        }
    }
}
