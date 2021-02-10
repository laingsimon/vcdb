using System.Collections.Generic;
using System.Linq;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;
using vcdb.Scripting.ExecutionPlan;
using vcdb.Scripting.ForeignKey;

namespace vcdb.SqlServer.Scripting
{
    public class SqlServerForeignKeyScriptBuilder : IForeignKeyScriptBuilder
    {
        private readonly IDescriptionScriptBuilder descriptionScriptBuilder;

        public SqlServerForeignKeyScriptBuilder(IDescriptionScriptBuilder descriptionScriptBuilder)
        {
            this.descriptionScriptBuilder = descriptionScriptBuilder;
        }

        public IEnumerable<IScriptTask> CreateUpgradeScripts(ObjectName requiredTableName, IReadOnlyCollection<ForeignKeyDifference> foreignKeyDifferences)
        {
            if (foreignKeyDifferences == null)
            {
                yield break;
            }

            foreach (var difference in foreignKeyDifferences)
            {
                if ((difference.ForeignKeyDeleted || difference.ChangedColumns?.Any() == true))
                {
                    yield return GetDropForeignKeyScript(requiredTableName, difference.CurrentForeignKey);
                }

                if ((difference.ForeignKeyAdded || difference.ChangedColumns?.Any() == true))
                {
                    yield return GetAddForeignKeyScripts(requiredTableName, difference.RequiredForeignKey);
                }

                if (difference.ForeignKeyRenamedTo != null)
                {
                    yield return GetRenameForeignKeyScript(requiredTableName, difference.CurrentForeignKey, difference.RequiredForeignKey);
                }

                if (difference.DescriptionChangedTo != null)
                {
                    yield return descriptionScriptBuilder.ChangeForeignKeyDescription(
                        requiredTableName,
                        difference.RequiredForeignKey.Key,
                        difference.CurrentForeignKey.Value.Description,
                        difference.RequiredForeignKey.Value.Description)
                        .Requiring().ForeignKeyReferencing(difference.RequiredForeignKey.Value.ReferencedTable).ToBeCreatedOrAltered();
                }
            }
        }

        private IScriptTask GetRenameForeignKeyScript(
            ObjectName requiredTableName,
            NamedItem<string, ForeignKeyDetails> currentForeignKey,
            NamedItem<string, ForeignKeyDetails> requiredForeignKey)
        {
            return new SqlScript($@"EXEC sp_rename 
    @objname = '{requiredTableName.Schema}.{currentForeignKey.Key}',
    @newname = '{requiredForeignKey.Key}', 
    @objtype = 'OBJECT'
GO").AsTask();
        }

        private IScriptTask GetAddForeignKeyScripts(ObjectName requiredTableName, NamedItem<string, ForeignKeyDetails> requiredForeignKey)
        {
            var columns = requiredForeignKey.Value.Columns;
            var sourceColumns = columns.Keys.Select(columnName => columnName.SqlSafeName());
            var referencedColumns = columns.Values.Select(columnName => columnName.SqlSafeName());

            var addForeignKey = new SqlScript($@"ALTER TABLE {requiredTableName.SqlSafeName()}
ADD CONSTRAINT {requiredForeignKey.Key.SqlSafeName()} 
FOREIGN KEY ({string.Join(", ", sourceColumns)}) 
REFERENCES {requiredForeignKey.Value.ReferencedTable.SqlSafeName()} ({string.Join(", ", referencedColumns)})
GO")
    .Requiring().Table(requiredTableName).ToBeCreatedOrAltered()
    .Requiring().Columns(requiredTableName.Components(columns.Keys)).ToBeCreatedOrAltered()
    .Requiring().Columns(requiredForeignKey.Value.ReferencedTable.Components(columns.Values)).ToBeCreatedOrAltered()
    .Requiring().PrimaryKeyOn(requiredForeignKey.Value.ReferencedTable).ToBeCreatedOrAltered()
    .CreatesOrAlters().ForeignKeyReferencing(requiredForeignKey.Value.ReferencedTable)
    .CreatesOrAlters().ForeignKeyOn(requiredTableName.Components(requiredForeignKey.Value.Columns.Keys));

            if (!string.IsNullOrEmpty(requiredForeignKey.Value.Description))
            {
                var addDescription = descriptionScriptBuilder.ChangeForeignKeyDescription(
                    requiredTableName,
                    requiredForeignKey.Key,
                    null,
                    requiredForeignKey.Value.Description);

                return new ScriptBlock(new[] { addForeignKey, addDescription });
            }

            return addForeignKey;
        }

        private IScriptTask GetDropForeignKeyScript(ObjectName requiredTableName, NamedItem<string, ForeignKeyDetails> currentForeignKey)
        {
            return new SqlScript($@"ALTER TABLE {requiredTableName.SqlSafeName()}
DROP CONSTRAINT {currentForeignKey.Key.SqlSafeName()}
GO")
    .Drops().ForeignKeyReferencing(currentForeignKey.Value.ReferencedTable)
    .Drops().ForeignKeyOn(requiredTableName.Components(currentForeignKey.Value.Columns.Keys));
        }
    }
}
