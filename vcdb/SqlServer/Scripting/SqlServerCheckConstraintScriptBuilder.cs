using System.Collections.Generic;
using System.Linq;
using vcdb.CommandLine;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;

namespace vcdb.SqlServer.Scripting
{
    public class SqlServerCheckConstraintScriptBuilder : ICheckConstraintScriptBuilder
    {
        private readonly Options options;
        private readonly ISqlObjectNameHelper objectNameHelper;

        public SqlServerCheckConstraintScriptBuilder(Options options, ISqlObjectNameHelper objectNameHelper)
        {
            this.options = options;
            this.objectNameHelper = objectNameHelper;
        }

        public IEnumerable<SqlScript> CreateUpgradeScripts(TableDifference tableDifference)
        {
            if (tableDifference.TableDeleted)
            {
                yield break;
            }

            var currentTableName = tableDifference.CurrentTable?.Key;
            var requiredTableName = tableDifference.RequiredTable.Key;
            var differentColumns = tableDifference.ColumnDifferences ?? new ColumnDifference[0];

            if (tableDifference.TableRenamedTo != null)
            {
                if (!options.IgnoreUnnamedConstraints)
                {
                    var CheckConstraintRenames = GetRenameUnnamedChecksIfNoColumnsAreRenamed(tableDifference);
                    foreach (var CheckConstraintRename in CheckConstraintRenames)
                        yield return CheckConstraintRename;
                }
            }

            foreach (var rename in differentColumns.Where(difference => difference.ColumnRenamedTo != null))
            {
                var requiredColumn = rename.RequiredColumn;
                var currentColumn = rename.CurrentColumn;

                if (rename.CheckChangedTo == null && requiredColumn.Value.Check != null && rename.CheckRenamedTo == null && currentColumn.Value.CheckName == null)
                {
                    //the column has a check and continues to have a Check. The check constraint does not have a user-provided name currently or as part of the upgrade
                    //as the column has changed name, the (automatically generated) name of the check should also change (to keep it consistent)

                    if (!options.IgnoreUnnamedConstraints)
                    {
                        //rename the Check too
                        yield return GetRenameCheckScript(
                            currentTableName,
                            requiredTableName,
                            currentColumn,
                            currentColumn.Value.CheckName,
                            requiredColumn,
                            requiredColumn.Value.CheckName);
                    }
                }
            }

            if (tableDifference.TableAdded)
            {
                foreach (var columnWithCheck in tableDifference.RequiredTable.Value.Columns.Where(col => col.Value.Check != null))
                {
                    foreach (var script in GetAddCheckScript(requiredTableName, columnWithCheck.Key, columnWithCheck.Value))
                        yield return script;
                }
            }
            else
            {
                foreach (var addedColumn in tableDifference.ColumnDifferences.Where(cd => cd.ColumnAdded))
                {
                    foreach (var script in GetAddCheckScript(
                        requiredTableName,
                        addedColumn.RequiredColumn.Key,
                        addedColumn.RequiredColumn.Value))
                    {
                        yield return script;
                    }
                }
            }
        }

        public IEnumerable<SqlScript> CreateUpgradeScripts(TableName tableName, ColumnDifference columnDifference)
        {
            if (columnDifference.CheckChangedTo != null)
            {
                if (columnDifference.CheckChangedTo.Value == null)
                    yield return GetDropCheckScript(tableName, GetCurrentConstraintName(tableName, columnDifference));
                else
                {
                    foreach (var script in GetAlterCheckScript(tableName, columnDifference.RequiredColumn, GetCurrentConstraintName(tableName, columnDifference)))
                        yield return script;
                }
            }
            else if (columnDifference.CheckRenamedTo != null)
            {
                yield return GetRenameCheckScript(
                    tableName,
                    tableName,
                    columnDifference.CurrentColumn,
                    columnDifference.CurrentColumn.Value.CheckName,
                    columnDifference.RequiredColumn,
                    columnDifference.CheckRenamedTo.Value);
            }

            if (columnDifference.ColumnAdded && columnDifference.RequiredColumn.Value.Check != null)
            {
                foreach (var script in GetAddCheckScript(
                    tableName,
                    columnDifference.RequiredColumn.Key,
                    columnDifference.RequiredColumn.Value))
                {
                    yield return script;
                }
            }
        }

        private string GetCurrentConstraintName(TableName tableName, ColumnDifference columnDifference)
        {
            var currentColumn = columnDifference.CurrentColumn;
            if (currentColumn.Value.CheckName != null)
                return currentColumn.Value.CheckName;

            return objectNameHelper.GetAutomaticConstraintName(
                "CK",
                tableName.Table,
                currentColumn.Key,
                currentColumn.Value.CheckObjectId.Value);
        }

        private IEnumerable<SqlScript> GetRenameUnnamedChecksIfNoColumnsAreRenamed(TableDifference tableDifference)
        {
            var requiredTable = tableDifference.RequiredTable;
            var columnsWithChecksButNoExplicitName = requiredTable.Value.Columns.Where(col =>
            {
                return col.Value.Check != null && col.Value.CheckName == null;
            });

            foreach (var requiredColumnWithUnnamedCheck in columnsWithChecksButNoExplicitName)
            {
                var columnDifference = tableDifference.ColumnDifferences.SingleOrDefault(cd => cd.RequiredColumn.Key == requiredColumnWithUnnamedCheck.Key);

                if (columnDifference != null)
                {
                    continue;
                }

                var currentColumn = new NamedItem<string, ColumnDetails>(
                    requiredColumnWithUnnamedCheck.Key,
                    tableDifference.CurrentTable.Value.Columns[requiredColumnWithUnnamedCheck.Key]);

                //The column hasn't changed (name) so the required and current column details are the same
                yield return GetRenameCheckScript(
                    tableDifference.CurrentTable.Key,
                    tableDifference.RequiredTable.Key,
                    currentColumn,
                    currentColumn.Value.CheckName,
                    requiredColumnWithUnnamedCheck.AsNamedItem(),
                    null);
            }
        }

        private string GetNameForColumnCheck(TableName tableName, NamedItem<string, ColumnDetails> column)
        {
            return objectNameHelper.GetAutomaticConstraintName("CK", tableName.Table, column.Key, column.Value.CheckObjectId ?? 0);
        }

        private SqlScript GetDropCheckScript(TableName tableName, string constraintName)
        {
            return new SqlScript($@"ALTER TABLE {tableName.SqlSafeName()}
DROP CONSTRAINT {constraintName.SqlSafeName()}
GO");
        }

        private IEnumerable<SqlScript> GetAlterCheckScript(TableName tableName, NamedItem<string, ColumnDetails> columnDetails, string currentConstraintName)
        {
            var columnName = columnDetails.Key;
            var column = columnDetails.Value;
            yield return GetDropCheckScript(tableName, currentConstraintName);
            foreach (var script in GetAddCheckScript(tableName, columnName, column))
            {
                yield return script;
            }
        }

        private IEnumerable<SqlScript> GetAddCheckScript(TableName tableName, string columnName, ColumnDetails column)
        {
            if (column.Check == null)
                yield break;

            var unnamedCheckConstraint = GetNameForColumnCheck(tableName, new NamedItem<string, ColumnDetails>(columnName, column));
            yield return new SqlScript($@"ALTER TABLE {tableName.SqlSafeName()}
ADD CONSTRAINT {(column.CheckName ?? unnamedCheckConstraint).SqlSafeName()}
CHECK ({column.Check})
GO");

            if (column.CheckName == null)
            {
                yield return new SqlScript(@$"DECLARE @newName VARCHAR(1024)
SELECT @newName = 'CK__{tableName.Table}__{columnName}__' + FORMAT(chk.OBJECT_ID, 'X')
FROM sys.check_constraints chk
INNER JOIN sys.columns col
ON col.column_id = chk.parent_column_id
AND col.object_id = chk.parent_object_id
INNER JOIN sys.tables tab
ON tab.object_id = col.object_id
WHERE tab.name = '{tableName.Table}'
AND SCHEMA_NAME(tab.schema_id) = '{tableName.Schema}'
AND chk.name = '{unnamedCheckConstraint}'

EXEC sp_rename 
    @objname = '{unnamedCheckConstraint}', 
    @newname = @newName, 
    @objtype = 'OBJECT'
GO");
            }
        }

        private SqlScript GetRenameCheckScript(
            TableName currentTableName,
            TableName requiredTableName,
            NamedItem<string, ColumnDetails> currentColumn,
            string currentConstraintName,
            NamedItem<string, ColumnDetails> requiredColumn,
            string requiredConstraintName)
        {
            var currentAutomaticConstraintName = objectNameHelper.GetAutomaticConstraintName(
                "CK",
                currentTableName.Table,
                currentColumn.Key,
                currentColumn.Value.CheckObjectId ?? 0);
            var requiredAutomaticConstraintName = objectNameHelper.GetAutomaticConstraintName(
                "CK",
                requiredTableName.Table,
                requiredColumn.Key,
                currentColumn.Value.CheckObjectId ?? 0);

            return new SqlScript(@$"EXEC sp_rename 
    @objname = '{currentTableName.Schema.SqlSafeName()}.{(currentConstraintName ?? currentAutomaticConstraintName).SqlSafeName()}', 
    @newname = '{(requiredConstraintName ?? requiredAutomaticConstraintName).SqlSafeName()}', 
    @objtype = 'OBJECT'
GO");
        }
    }
}
