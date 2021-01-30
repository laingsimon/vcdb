using System.Collections.Generic;
using vcdb.Models;
using vcdb.Scripting.Column;

namespace vcdb.Scripting
{
    public class ComparerContext
    {
        public DatabaseDetails CurrentDatabase { get; }
        public DatabaseDetails RequiredDatabase { get; }
        public NamedItem<ObjectName, TableDetails> CurrentTable { get; }
        public NamedItem<ObjectName, TableDetails> RequiredTable { get; }
        public IReadOnlyCollection<ColumnDifference> ColumnDifferences { get; }

        public ComparerContext()
        { }

        private ComparerContext(ComparerContext rootContext)
        {
            CurrentDatabase = rootContext.CurrentDatabase;
            RequiredDatabase = rootContext.RequiredDatabase;
        }

        private ComparerContext(
            ComparerContext rootContext,
            DatabaseDetails currentDatabase,
            DatabaseDetails requiredDatabase)
            : this(rootContext)
        {
            CurrentDatabase = currentDatabase;
            RequiredDatabase = requiredDatabase;
        }

        private ComparerContext(
            ComparerContext rootContext,
            NamedItem<ObjectName, TableDetails> currentTable,
            NamedItem<ObjectName, TableDetails> requiredTable,
            IReadOnlyCollection<ColumnDifference> columnDifferences = null)
            :this(rootContext)
        {
            CurrentTable = currentTable;
            RequiredTable = requiredTable;
            ColumnDifferences = columnDifferences;
        }

        public ComparerContext ForDatabase(
            DatabaseDetails currentDatabase,
            DatabaseDetails requiredDatabase)
        {
            return new ComparerContext(this, currentDatabase, requiredDatabase);
        }

        public ComparerContext ForTable(
            NamedItem<ObjectName, TableDetails> currentTable,
            NamedItem<ObjectName, TableDetails> requiredTable,
            IReadOnlyCollection<ColumnDifference> columnDifferences = null)
        {
            return new ComparerContext(this, currentTable, requiredTable, columnDifferences);
        }
    }
}
