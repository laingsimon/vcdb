﻿using System.Collections.Generic;
using System.Linq;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting.CheckConstraint;
using vcdb.Scripting.Column;
using vcdb.Scripting.Table;

namespace vcdb.SqlServer.Scripting
{
    public class SqlServerCheckConstraintScriptBuilder : ICheckConstraintScriptBuilder
    {
        private readonly ISqlObjectNameHelper objectNameHelper;
        private readonly IHashHelper hashHelper;

        public SqlServerCheckConstraintScriptBuilder(
            ISqlObjectNameHelper objectNameHelper,
            IHashHelper hashHelper)
        {
            this.objectNameHelper = objectNameHelper;
            this.hashHelper = hashHelper;
        }

        public IEnumerable<SqlScript> CreateUpgradeScriptsBeforeColumnChanges(TableDifference tableDifference, ColumnDifference alteration)
        {
            //drop any check constraints on columns that need to be renamed

            if (tableDifference.TableRenamedTo != null)
            {
                foreach (var script in DropAllUnnamedCheckConstraints(tableDifference))
                {
                    yield return script;
                }
                yield break;
            }

            foreach (var columnToDropOrRename in tableDifference.ColumnDifferences.Where(cd => cd.ColumnDeleted || cd.ColumnRenamedTo != null))
            {
                var checkConstraintsForColumn = GetCheckConstraintsBoundToCurrentColumn(tableDifference, columnToDropOrRename.CurrentColumn.Key);

                foreach (var checkConstraint in checkConstraintsForColumn)
                {
                    yield return DropCheckConstraint(tableDifference.CurrentTable.Key, checkConstraint);
                }
            }
        }

        private SqlScript DropCheckConstraint(TableName tableName, CheckConstraintDetails checkConstraint)
        {
            return new SqlScript($@"ALTER TABLE {tableName.SqlSafeName()}
DROP CONSTRAINT {checkConstraint.SqlName.SqlSafeName()}
GO");
        }

        private IEnumerable<CheckConstraintDetails> GetCheckConstraintsBoundToCurrentColumn(TableDifference tableDifference, string currentColumnName)
        {
            return tableDifference.CurrentTable.Value.Checks.OrEmptyCollection()
                .Where(check => check.ColumnNames.Any(columnName => columnName == currentColumnName));
        }

        private IEnumerable<SqlScript> DropAllUnnamedCheckConstraints(TableDifference tableDifference)
        {
            if (tableDifference.CurrentTable.Value.Checks == null)
            {
                yield break;
            }

            foreach (var checkConstraint in tableDifference.CurrentTable.Value.Checks.Where(check => check.Name == null))
            {
                yield return DropCheckConstraint(tableDifference.RequiredTable.Key, checkConstraint);
            }
        }

        public IEnumerable<SqlScript> CreateUpgradeScripts(TableDifference tableDifference)
        {
            if (tableDifference.TableDeleted)
            {
                yield break;
            }

            var tableName = tableDifference.RequiredTable.Key;
            if (tableDifference.TableRenamedTo != null)
            {
                foreach (var script in CreateRenameAllCheckConstraintScripts(tableDifference))
                {
                    yield return script;
                }

                foreach (var script in CreateAddAllCheckConstraintScripts(tableDifference))
                {
                    yield return script;
                }
            }

            var processedCheckConstraintDifferences = new HashSet<CheckConstraintDifference>();
            foreach (var columnToDropOrRename in tableDifference.ColumnDifferences.OrEmptyCollection().Where(cd => cd.ColumnRenamedTo != null))
            {
                var checkConstraintsForColumn = GetCheckConstraintsBoundToCurrentColumn(tableDifference, columnToDropOrRename.CurrentColumn.Key);

                foreach (var checkConstraint in checkConstraintsForColumn)
                {
                    var checkConstraintDifference = tableDifference.ChangedCheckConstraints
                        .SingleOrDefault(diff => diff.CurrentConstraint == checkConstraint);

                    if (checkConstraintDifference != null && checkConstraintDifference.RequiredConstraint != null)
                    {
                        processedCheckConstraintDifferences.Add(checkConstraintDifference);
                        foreach (var script in AddCheckConstraint(tableName, checkConstraintDifference))
                        {
                            yield return script;
                        }
                    }
                }
            }

            foreach (var changedCheckConstraint in tableDifference.ChangedCheckConstraints.OrEmptyCollection().Except(processedCheckConstraintDifferences))
            {
                if (changedCheckConstraint.ConstraintAdded)
                {
                    if (tableDifference.TableRenamedTo != null)
                    {
                        continue;
                    }

                    processedCheckConstraintDifferences.Add(changedCheckConstraint);
                    foreach (var script in AddCheckConstraint(tableName, changedCheckConstraint))
                    {
                        yield return script;
                    }
                }

                var columnWasRenamed = changedCheckConstraint?.CurrentConstraint?.ColumnNames.OrEmptyCollection()
                    .Any(columnName =>
                        tableDifference.ColumnDifferences.Any(columnDifference =>
                            columnDifference.CurrentColumn.Key == columnName
                            && columnDifference.ColumnRenamedTo != null)) == true;

                if (changedCheckConstraint.ConstraintDeleted)
                {
                    processedCheckConstraintDifferences.Add(changedCheckConstraint);
                    //is the check constraint bound to a column that has been renamed?
                    //if so then the constraint has already been deleted

                    if (!columnWasRenamed)
                    {
                        //check has aleady been dropped (in the before column rename scripts)
                        yield return DropCheckConstraint(tableName, changedCheckConstraint.CurrentConstraint);
                    }
                }

                if (changedCheckConstraint.CheckRenamedTo != null)
                {
                    processedCheckConstraintDifferences.Add(changedCheckConstraint);
                    if (columnWasRenamed)
                    {
                        foreach (var script in AddCheckConstraint(tableName, changedCheckConstraint.RequiredConstraint))
                        {
                            yield return script;
                        }
                    }
                    else
                    {
                        yield return RenameCheckConstraint(
                            tableName.Schema,
                            changedCheckConstraint.CurrentConstraint.SqlName,
                            GetNameForCheckConstraint(tableName, changedCheckConstraint.RequiredConstraint));
                    }
                }

                if (changedCheckConstraint.CheckChangedTo != null)
                {
                    processedCheckConstraintDifferences.Add(changedCheckConstraint);
                    yield return DropCheckConstraint(tableName, changedCheckConstraint.CurrentConstraint);
                    foreach (var script in AddCheckConstraint(tableName, changedCheckConstraint))
                    {
                        yield return script;
                    }
                }
            }
        }

        private SqlScript RenameCheckConstraint(string currentSchema, string currentName, string requiredName)
        {
            return new SqlScript($@"EXEC sp_rename 
    @objname = '{currentSchema}.{currentName}', 
    @newname = '{requiredName}', 
    @objtype = 'OBJECT'
GO");
        }

        private IEnumerable<SqlScript> CreateRenameAllCheckConstraintScripts(TableDifference tableDifference)
        {
            foreach (var checkConstraint in tableDifference.RequiredTable.Value.Checks.OrEmptyCollection())
            {
                //if checkConstraint has been changed in any form then don't do anything, it needs to be readded, not renamed
                var checkDifference = tableDifference.ChangedCheckConstraints.OrEmptyCollection()
                    .SingleOrDefault(difference => difference.RequiredConstraint == checkConstraint);

                if (checkDifference != null)
                {
                    //something has changed, the constraint needs to be dropped and recreated rather than simply renamed
                    continue;
                }

                var currentConstraint = tableDifference.CurrentTable.Value.Checks.SingleOrDefault(check => check.Check == checkConstraint.Check);
                var newAutomaticName = objectNameHelper.GetAutomaticConstraintName(
                    "CK",
                    tableDifference.RequiredTable.Key.Table,
                    currentConstraint.ColumnNames.Length == 1
                        ? currentConstraint.ColumnNames.Single()
                        : null,
                    currentConstraint.CheckObjectId.Value);

                yield return RenameCheckConstraint(
                    tableDifference.RequiredTable.Key.Schema,
                    currentConstraint.SqlName,
                    checkConstraint.Name ?? newAutomaticName);
            }
        }

        private IEnumerable<SqlScript> CreateAddAllCheckConstraintScripts(TableDifference tableDifference)
        {
            foreach (var checkConstraint in tableDifference.RequiredTable.Value.Checks.OrEmptyCollection())
            {
                //if checkConstraint has been changed in any form then add it, otherwise dont.
                var checkDifference = tableDifference.ChangedCheckConstraints.OrEmptyCollection()
                    .SingleOrDefault(difference => difference.RequiredConstraint == checkConstraint);

                if (checkDifference == null || checkDifference.CurrentConstraint?.Name != null)
                {
                    //no change to the check
                    continue;
                }

                foreach (var script in AddCheckConstraint(tableDifference.RequiredTable.Key, checkConstraint))
                {
                    yield return script;
                }
            }
        }

        private IEnumerable<SqlScript> AddCheckConstraint(TableName tableName, CheckConstraintDifference checkDifference)
        {
            return AddCheckConstraint(tableName, checkDifference.RequiredConstraint);
        }

        private IEnumerable<SqlScript> AddCheckConstraint(
            TableName tableName,
            CheckConstraintDetails checkConstraint)
        {
            var unnamedCheckConstraint = GetNameForCheckConstraint(tableName, checkConstraint);

            yield return new SqlScript($@"ALTER TABLE {tableName.SqlSafeName()}
ADD CONSTRAINT {(checkConstraint.Name ?? unnamedCheckConstraint).SqlSafeName()}
CHECK ({checkConstraint.Check})
GO");

            if (checkConstraint.Name == null)
            {
                yield return new SqlScript(@$"DECLARE @newName VARCHAR(1024)
SELECT @newName = 'CK__{tableName.Table}__' + col.name + '__' + FORMAT(chk.OBJECT_ID, 'X')
FROM sys.check_constraints chk
INNER JOIN sys.columns col
ON col.column_id = chk.parent_column_id
AND col.object_id = chk.parent_object_id
INNER JOIN sys.tables tab
ON tab.object_id = chk.parent_object_id
WHERE tab.name = '{tableName.Table}'
AND SCHEMA_NAME(tab.schema_id) = '{tableName.Schema}'
AND chk.name = '{unnamedCheckConstraint}'

EXEC sp_rename 
    @objname = '{tableName.Schema}.{unnamedCheckConstraint}', 
    @newname = @newName, 
    @objtype = 'OBJECT'
GO");
            }
        }

        private string GetNameForCheckConstraint(
            TableName tableName, 
            CheckConstraintDetails checkConstraint)
        {
            if (!string.IsNullOrEmpty(checkConstraint.Name))
                return checkConstraint.Name;

            return objectNameHelper.GetAutomaticConstraintName(
                "CK",
                tableName.Table,
                GetDeterministicIdentifierForNewCheckConstraint(checkConstraint),
                checkConstraint.CheckObjectId ?? 0);
        }

        /// <summary>
        /// Return a value that is deterministically created, but unique to this check constraint
        /// So that it can be used as an identifier if/when the constraint needs to be renamed later
        /// </summary>
        /// <param name="checkConstraint"></param>
        /// <returns></returns>
        private string GetDeterministicIdentifierForNewCheckConstraint(CheckConstraintDetails checkConstraint)
        {
            return hashHelper.GetHash(checkConstraint.Check);
        }
    }
}