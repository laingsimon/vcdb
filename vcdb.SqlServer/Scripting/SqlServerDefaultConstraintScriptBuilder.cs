using System.Collections.Generic;
using System.Linq;
using vcdb.CommandLine;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;
using vcdb.Scripting.Column;
using vcdb.Scripting.Table;

namespace vcdb.SqlServer.Scripting
{
    public class SqlServerDefaultConstraintScriptBuilder : IDefaultConstraintScriptBuilder
    {
        private readonly Options options;
        private readonly ISqlObjectNameHelper objectNameHelper;

        public SqlServerDefaultConstraintScriptBuilder(Options options, ISqlObjectNameHelper objectNameHelper)
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
                    var defaultConstraintRenames = GetRenameUnnamedDefaultsIfNoColumnsAreRenamed(tableDifference);
                    foreach (var defaultConstraintRename in defaultConstraintRenames)
                        yield return defaultConstraintRename;
                }
            }

            foreach (var rename in differentColumns.Where(difference => difference.ColumnRenamedTo != null))
            {
                var requiredColumn = rename.RequiredColumn;
                var currentColumn = rename.CurrentColumn;

                if (rename.DefaultChangedTo == null && requiredColumn.Value.Default != null && rename.DefaultRenamedTo == null && currentColumn.Value.DefaultName == null)
                {
                    //the column has a default and continues to have a default. The default constraint does not have a user-provided name currently or as part of the upgrade
                    //as the column has changed name, the (automatically generated) name of the default should also change (to keep it consistent)

                    if (!options.IgnoreUnnamedConstraints)
                    {
                        //rename the default too
                        yield return GetRenameDefaultScript(
                            currentTableName,
                            requiredTableName,
                            currentColumn,
                            currentColumn.Value.SqlDefaultName,
                            requiredColumn,
                            requiredColumn.Value.DefaultName);
                    }
                }
            }

            if (tableDifference.TableAdded)
            {
                foreach (var columnWithDefault in tableDifference.RequiredTable.Value.Columns.Where(col => col.Value.Default != null))
                {
                    foreach (var script in GetAddDefaultScript(requiredTableName, columnWithDefault.Key, columnWithDefault.Value))
                        yield return script;
                }
            }
            else
            {
                foreach (var addedColumn in tableDifference.ColumnDifferences.Where(cd => cd.ColumnAdded))
                {
                    foreach (var script in GetAddDefaultScript(
                        requiredTableName,
                        addedColumn.RequiredColumn.Key,
                        addedColumn.RequiredColumn.Value))
                    {
                        yield return script;
                    }
                }
            }
        }

        public IEnumerable<SqlScript> CreateUpgradeScripts(ObjectName tableName, ColumnDifference columnDifference)
        {
            if (columnDifference.DefaultChangedTo != null)
            {
                if (columnDifference.DefaultChangedTo.Value == null)
                    yield return GetDropDefaultScript(tableName, columnDifference.CurrentColumn.Value.SqlDefaultName);
                else
                {
                    foreach (var script in GetAlterDefaultScript(tableName, columnDifference.RequiredColumn, columnDifference.CurrentColumn.Value.SqlDefaultName))
                        yield return script;
                }
            }
            else if (columnDifference.DefaultRenamedTo != null)
            {
                yield return GetRenameDefaultScript(
                    tableName,
                    tableName,
                    columnDifference.CurrentColumn,
                    columnDifference.CurrentColumn.Value.SqlDefaultName,
                    columnDifference.RequiredColumn,
                    columnDifference.DefaultRenamedTo.Value);
            }

            if (columnDifference.ColumnAdded && columnDifference.RequiredColumn.Value.Default != null)
            {
                foreach (var script in GetAddDefaultScript(
                    tableName,
                    columnDifference.RequiredColumn.Key,
                    columnDifference.RequiredColumn.Value))
                {
                    yield return script;
                }
            }
        }

        private IEnumerable<SqlScript> GetRenameUnnamedDefaultsIfNoColumnsAreRenamed(TableDifference tableDifference)
        {
            var requiredTable = tableDifference.RequiredTable;
            var columnsWithDefaultsButNoExplicitName = requiredTable.Value.Columns.Where(col =>
            {
                return col.Value.Default != null && col.Value.DefaultName == null;
            });

            foreach (var requiredColumnWithUnnamedDefault in columnsWithDefaultsButNoExplicitName)
            {
                var columnDifference = tableDifference.ColumnDifferences.SingleOrDefault(cd => cd.RequiredColumn.Key == requiredColumnWithUnnamedDefault.Key);

                if (columnDifference != null)
                {
                    continue;
                }

                var currentColumn = new NamedItem<string, ColumnDetails>(
                    requiredColumnWithUnnamedDefault.Key,
                    tableDifference.CurrentTable.Value.Columns[requiredColumnWithUnnamedDefault.Key]);

                //The column hasn't changed (name) so the required and current column details are the same
                yield return GetRenameDefaultScript(
                    tableDifference.CurrentTable.Key,
                    tableDifference.RequiredTable.Key,
                    currentColumn,
                    currentColumn.Value.SqlDefaultName,
                    requiredColumnWithUnnamedDefault.AsNamedItem(),
                    null);
            }
        }

        private string GetNameForColumnDefault(ObjectName tableName, NamedItem<string, ColumnDetails> column)
        {
            return objectNameHelper.GetAutomaticConstraintName("DF", tableName.Name, column.Key, column.Value.DefaultObjectId ?? 0);
        }

        private SqlScript GetDropDefaultScript(ObjectName tableName, string constraintName)
        {
            return new SqlScript($@"ALTER TABLE {tableName.SqlSafeName()}
DROP CONSTRAINT {constraintName.SqlSafeName()}
GO");
        }

        private IEnumerable<SqlScript> GetAlterDefaultScript(ObjectName tableName, NamedItem<string, ColumnDetails> columnDetails, string currentConstraintName)
        {
            var columnName = columnDetails.Key;
            var column = columnDetails.Value;
            yield return GetDropDefaultScript(tableName, currentConstraintName);
            foreach (var script in GetAddDefaultScript(tableName, columnName, column))
            {
                yield return script;
            }
        }

        private IEnumerable<SqlScript> GetAddDefaultScript(ObjectName tableName, string columnName, ColumnDetails column)
        {
            if (column.Default == null)
                yield break;

            var unnamedDefaultConstraint = GetNameForColumnDefault(tableName, new NamedItem<string, ColumnDetails>(columnName, column));
            yield return new SqlScript($@"ALTER TABLE {tableName.SqlSafeName()}
ADD CONSTRAINT {(column.DefaultName ?? unnamedDefaultConstraint).SqlSafeName()}
DEFAULT ({GetDefaultValue(column.Default)})
FOR {columnName.SqlSafeName()}
GO");

            if (column.DefaultName == null)
            {
                yield return new SqlScript(@$"DECLARE @newName VARCHAR(1024)
SELECT @newName = 'DF__{tableName.Name}__{columnName}__' + FORMAT(def.OBJECT_ID, 'X')
FROM sys.default_constraints def
INNER JOIN sys.columns col
ON col.column_id = def.parent_column_id
AND col.object_id = def.parent_object_id
INNER JOIN sys.tables tab
ON tab.object_id = col.object_id
WHERE tab.name = '{tableName.Name}'
AND SCHEMA_NAME(tab.schema_id) = '{tableName.Schema}'
AND def.name = '{unnamedDefaultConstraint}'

EXEC sp_rename 
    @objname = '{unnamedDefaultConstraint}', 
    @newname = @newName, 
    @objtype = 'OBJECT'
GO");
            }
        }

        private object GetDefaultValue(object defaultValue)
        {
            if (defaultValue is string)
                return $"'{defaultValue}'";

            return defaultValue;
        }

        private SqlScript GetRenameDefaultScript(
            ObjectName currentTableName,
            ObjectName requiredTableName,
            NamedItem<string, ColumnDetails> currentColumn,
            string currentConstraintName,
            NamedItem<string, ColumnDetails> requiredColumn,
            string requiredConstraintName)
        {
            var requiredAutomaticConstraintName = objectNameHelper.GetAutomaticConstraintName(
                "DF",
                requiredTableName.Name,
                requiredColumn.Key,
                currentColumn.Value.DefaultObjectId ?? 0);

            return new SqlScript(@$"EXEC sp_rename 
    @objname = '{currentTableName.Schema.SqlSafeName()}.{(currentConstraintName).SqlSafeName()}', 
    @newname = '{(requiredConstraintName ?? requiredAutomaticConstraintName).SqlSafeName()}', 
    @objtype = 'OBJECT'
GO");
        }
    }
}
