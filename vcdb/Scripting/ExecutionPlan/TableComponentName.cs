using System;
using System.Diagnostics;

namespace vcdb.Scripting.ExecutionPlan
{
    [DebuggerDisplay("{DebugString()}")]
    public class TableComponentName : IEquatable<TableComponentName>
    {
        public ObjectName Table { get; }
        public string Component { get; }

        public TableComponentName(ObjectName table, string component)
        {
            if (string.IsNullOrEmpty(component))
            {
                throw new ArgumentNullException(nameof(component));
            }

            Table = table ?? throw new ArgumentNullException(nameof(table));
            Component = component;
        }

        public bool Equals(TableComponentName other)
        {
            return other != null
                         && other.Table.Equals(Table)
                         && other.Component.Equals(Component);
        }

        public override bool Equals(object other)
        {
            return Equals(other as TableComponentName);
        }

        public override int GetHashCode()
        {
            return Table.GetHashCode()
            ^ Component.GetHashCode();
        }

        internal string DebugString()
        {
            return $"{Table.DebugString()}.{Table.nameConverter.ConvertToString(Component)}";
        }
    }
}