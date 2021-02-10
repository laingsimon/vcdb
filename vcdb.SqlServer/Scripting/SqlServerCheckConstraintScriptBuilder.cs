using System.Collections.Generic;
using System.Linq;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting.CheckConstraint;
using vcdb.Scripting.ExecutionPlan;
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

        public IEnumerable<IScriptTask> CreateUpgradeScripts(TableDifference tableDifference)
        {
            if (tableDifference.TableDeleted)
            {
                yield break;
            }

            //drop any check constraints on columns that need to be renamed
            var tableName = tableDifference.RequiredTable.Key;

            if (tableDifference.TableRenamedTo != null)
            {
                yield return new ScriptBlock(CreateRenameAllCheckConstraintScripts(tableDifference));
                yield return new ScriptBlock(CreateAddAllCheckConstraintScripts(tableDifference));
            }

            var processedCheckConstraintDifferences = new HashSet<CheckConstraintDifference>();
            foreach (var columnToDropOrRename in tableDifference.ColumnDifferences.OrEmptyCollection().Where(cd => cd.ColumnRenamedTo != null))
            {
                if (tableDifference.TableRenamedTo != null)
                {
                    yield return new ScriptBlock(DropAllUnnamedCheckConstraints(tableDifference));
                }

                var checkConstraintsForColumn = GetCheckConstraintsBoundToCurrentColumn(tableDifference, columnToDropOrRename.CurrentColumn.Key);

                if (columnToDropOrRename.ColumnDeleted)
                {
                    yield return new ScriptBlock(checkConstraintsForColumn.Select(checkConstraint => DropCheckConstraint(tableDifference.CurrentTable.Key, checkConstraint)));
                }
                else
                {
                    foreach (var checkConstraint in checkConstraintsForColumn)
                    {
                        var checkConstraintDifference = tableDifference.ChangedCheckConstraints.SingleOrDefault(diff => diff.CurrentConstraint == checkConstraint);

                        if (checkConstraintDifference != null)
                        {
                            processedCheckConstraintDifferences.Add(checkConstraintDifference);
                            if (checkConstraintDifference.CurrentConstraint.Name != null || tableDifference.TableRenamedTo == null)
                            {
                                //if it was unnamed, then it would have been dropped in the call to DropAllUnnamedCheckConstraints
                                yield return DropCheckConstraint(tableDifference.CurrentTable.Key, checkConstraint);
                            }
                            if (checkConstraintDifference.RequiredConstraint != null)
                            {
                                yield return AddCheckConstraint(tableName, checkConstraintDifference);
                            }
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
                    yield return AddCheckConstraint(tableName, changedCheckConstraint);
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
                        yield return AddCheckConstraint(tableName, changedCheckConstraint.RequiredConstraint);
                    }
                    else
                    {
                        yield return RenameCheckConstraint(
                            tableName,
                            changedCheckConstraint.CurrentConstraint.SqlName,
                            GetNameForCheckConstraint(tableName, changedCheckConstraint.RequiredConstraint));
                    }
                }

                if (changedCheckConstraint.CheckChangedTo != null)
                {
                    processedCheckConstraintDifferences.Add(changedCheckConstraint);
                    yield return DropCheckConstraint(tableName, changedCheckConstraint.CurrentConstraint);
                    yield return AddCheckConstraint(tableName, changedCheckConstraint);
                }
            }
        }

        private IEnumerable<CheckConstraintDetails> GetCheckConstraintsBoundToCurrentColumn(TableDifference tableDifference, string currentColumnName)
        {
            return tableDifference.CurrentTable.Value.Checks.OrEmptyCollection()
                .Where(check => check.ColumnNames.Any(columnName => columnName == currentColumnName));
        }

        private IEnumerable<IScriptTask> DropAllUnnamedCheckConstraints(TableDifference tableDifference)
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

        private IScriptTask RenameCheckConstraint(ObjectName tableName, string currentName, string requiredName)
        {
            return new SqlScript($@"EXEC sp_rename 
    @objname = '{tableName.Schema}.{currentName}', 
    @newname = '{requiredName}', 
    @objtype = 'OBJECT'
GO").CreatesOrAlters().CheckConstraintOn(tableName.Components());
        }

        private IEnumerable<IScriptTask> CreateRenameAllCheckConstraintScripts(TableDifference tableDifference)
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
                    tableDifference.RequiredTable.Key.Name,
                    currentConstraint.ColumnNames.Length == 1
                        ? currentConstraint.ColumnNames.Single()
                        : null,
                    currentConstraint.CheckObjectId.Value);

                yield return RenameCheckConstraint(
                    tableDifference.RequiredTable.Key,
                    currentConstraint.SqlName,
                    checkConstraint.Name ?? newAutomaticName);
            }
        }

        private IEnumerable<IScriptTask> CreateAddAllCheckConstraintScripts(TableDifference tableDifference)
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

                yield return AddCheckConstraint(tableDifference.RequiredTable.Key, checkConstraint);
            }
        }

        private IScriptTask DropCheckConstraint(ObjectName tableName, CheckConstraintDetails checkConstraint)
        {
            return new SqlScript($@"ALTER TABLE {tableName.SqlSafeName()}
DROP CONSTRAINT {checkConstraint.SqlName.SqlSafeName()}
GO").Drops().CheckConstraintOn(tableName.Components(checkConstraint.ColumnNames));
        }

        private IScriptTask AddCheckConstraint(ObjectName tableName, CheckConstraintDifference checkDifference)
        {
            return AddCheckConstraint(tableName, checkDifference.RequiredConstraint);
        }

        private IScriptTask AddCheckConstraint(
            ObjectName tableName,
            CheckConstraintDetails checkConstraint)
        {
            var unnamedCheckConstraint = GetNameForCheckConstraint(tableName, checkConstraint);

            var addConstraint = new SqlScript($@"ALTER TABLE {tableName.SqlSafeName()}
ADD CONSTRAINT {(checkConstraint.Name ?? unnamedCheckConstraint).SqlSafeName()}
CHECK ({checkConstraint.Check})
GO").Requiring().Table(tableName).ToBeCreatedOrAltered()
    .CreatesOrAlters().CheckConstraintOn(tableName.Components(checkConstraint.ColumnNames.OrEmptyCollection()));

            if (checkConstraint.Name == null)
            {
                var renameConstraint = new SqlScript(@$"DECLARE @newName VARCHAR(1024)
SELECT @newName = 'CK__{tableName.Name}__' + col.name + '__' + FORMAT(chk.OBJECT_ID, 'X')
FROM sys.check_constraints chk
INNER JOIN sys.columns col
ON col.column_id = chk.parent_column_id
AND col.object_id = chk.parent_object_id
INNER JOIN sys.tables tab
ON tab.object_id = chk.parent_object_id
WHERE tab.name = '{tableName.Name}'
AND SCHEMA_NAME(tab.schema_id) = '{tableName.Schema}'
AND chk.name = '{unnamedCheckConstraint}'

EXEC sp_rename 
    @objname = '{tableName.Schema}.{unnamedCheckConstraint}', 
    @newname = @newName, 
    @objtype = 'OBJECT'
GO").Requiring().Table(tableName).ToBeCreatedOrAltered()
    .CreatesOrAlters().CheckConstraintOn(tableName.Components(checkConstraint.ColumnNames.OrEmptyCollection()));

                return new ScriptBlock(new[] { addConstraint, renameConstraint });
            }

            return addConstraint;
        }

        private string GetNameForCheckConstraint(
            ObjectName tableName,
            CheckConstraintDetails checkConstraint)
        {
            if (!string.IsNullOrEmpty(checkConstraint.Name))
                return checkConstraint.Name;

            return objectNameHelper.GetAutomaticConstraintName(
                "CK",
                tableName.Name,
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