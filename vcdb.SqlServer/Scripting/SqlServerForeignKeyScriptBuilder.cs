using System.Collections.Generic;
using System.Linq;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;
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

        public IEnumerable<IOutputable> CreateUpgradeScripts(ObjectName requiredTableName, IReadOnlyCollection<ForeignKeyDifference> foreignKeyDifferences, ScriptingPhase phase)
        {
            if (foreignKeyDifferences == null)
            {
                yield break;
            }

            foreach (var difference in foreignKeyDifferences)
            {
                if ((difference.ForeignKeyDeleted || difference.ChangedColumns?.Any() == true) && phase == ScriptingPhase.DropReferences)
                {
                    yield return GetDropForeignKeyScript(requiredTableName, difference.CurrentForeignKey);
                }

                if ((difference.ForeignKeyAdded || difference.ChangedColumns?.Any() == true) && phase == ScriptingPhase.Recreate)
                {
                    yield return new OutputableCollection(GetAddForeignKeyScripts(requiredTableName, difference.RequiredForeignKey));
                }

                if (difference.ForeignKeyRenamedTo != null && phase == ScriptingPhase.Recreate)
                {
                    yield return GetRenameForeignKeyScript(requiredTableName, difference.CurrentForeignKey, difference.RequiredForeignKey);
                }

                if (difference.DescriptionChangedTo != null && phase == ScriptingPhase.Recreate)
                {
                    yield return descriptionScriptBuilder.ChangeForeignKeyDescription(
                        requiredTableName,
                        difference.RequiredForeignKey.Key,
                        difference.CurrentForeignKey.Value.Description,
                        difference.RequiredForeignKey.Value.Description);
                }
            }
        }

        private SqlScript GetRenameForeignKeyScript(
            ObjectName requiredTableName,
            NamedItem<string, ForeignKeyDetails> currentForeignKey,
            NamedItem<string, ForeignKeyDetails> requiredForeignKey)
        {
            return new SqlScript($@"EXEC sp_rename 
    @objname = '{requiredTableName.Schema}.{currentForeignKey.Key}',
    @newname = '{requiredForeignKey.Key}', 
    @objtype = 'OBJECT'
GO");
        }

        private IEnumerable<IOutputable> GetAddForeignKeyScripts(ObjectName requiredTableName, NamedItem<string, ForeignKeyDetails> requiredForeignKey)
        {
            var columns = requiredForeignKey.Value.Columns;
            var sourceColumns = columns.Keys.Select(columnName => columnName.SqlSafeName());
            var referencedColumns = columns.Values.Select(columnName => columnName.SqlSafeName());

            yield return new SqlScript($@"ALTER TABLE {requiredTableName.SqlSafeName()}
ADD CONSTRAINT {requiredForeignKey.Key.SqlSafeName()} 
FOREIGN KEY ({string.Join(", ", sourceColumns)}) 
REFERENCES {requiredForeignKey.Value.ReferencedTable.SqlSafeName()} ({string.Join(", ", referencedColumns)})
GO");

            if (!string.IsNullOrEmpty(requiredForeignKey.Value.Description))
            {
                yield return descriptionScriptBuilder.ChangeForeignKeyDescription(
                    requiredTableName,
                    requiredForeignKey.Key,
                    null,
                    requiredForeignKey.Value.Description);
            }
        }

        private SqlScript GetDropForeignKeyScript(ObjectName requiredTableName, NamedItem<string, ForeignKeyDetails> currentForeignKey)
        {
            return new SqlScript($@"ALTER TABLE {requiredTableName.SqlSafeName()}
DROP CONSTRAINT {currentForeignKey.Key.SqlSafeName()}
GO");
        }
    }
}
