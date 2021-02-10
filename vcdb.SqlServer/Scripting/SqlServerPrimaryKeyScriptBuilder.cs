using System.Collections.Generic;
using System.Linq;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;
using vcdb.Scripting.ExecutionPlan;
using vcdb.Scripting.PrimaryKey;

namespace vcdb.SqlServer.Scripting
{
    public class SqlServerPrimaryKeyScriptBuilder : IPrimaryKeyScriptBuilder
    {
        private const string PrimaryKeyObjectIdPrefix = "3214EC07";

        private readonly IDescriptionScriptBuilder descriptionScriptBuilder;
        private readonly ISqlObjectNameHelper objectNameHelper;
        private readonly IHashHelper hashHelper;

        public SqlServerPrimaryKeyScriptBuilder(
            IDescriptionScriptBuilder descriptionScriptBuilder,
            ISqlObjectNameHelper objectNameHelper,
            IHashHelper hashHelper)
        {
            this.descriptionScriptBuilder = descriptionScriptBuilder;
            this.objectNameHelper = objectNameHelper;
            this.hashHelper = hashHelper;
        }

        public IEnumerable<IScriptTask> CreateUpgradeScripts(ObjectName tableName, PrimaryKeyDifference primaryKeyDifference)
        {
            if (primaryKeyDifference == null)
            {
                yield break;
            }

            if (primaryKeyDifference.PrimaryKeyRemoved)
            {
                yield return GetDropPrimaryKeyScript(tableName, primaryKeyDifference.CurrentPrimaryKey);
                yield break;
            }
            else if (primaryKeyDifference.CurrentPrimaryKey != null && (primaryKeyDifference.ClusteredChangedTo != null || primaryKeyDifference.ColumnsAdded.Any() || primaryKeyDifference.ColumnsRemoved.Any()))
            {
                yield return GetDropPrimaryKeyScript(tableName, primaryKeyDifference.CurrentPrimaryKey);
            }

            if (!primaryKeyDifference.PrimaryKeyRemoved && (primaryKeyDifference.PrimaryKeyAdded || primaryKeyDifference.ClusteredChangedTo != null || primaryKeyDifference.ColumnsAdded.Any() || primaryKeyDifference.ColumnsRemoved.Any()))
            {
                yield return new MultiScriptTask(GetAddPrimaryKeyScripts(tableName, primaryKeyDifference));
            }

            if (primaryKeyDifference.RenamedTo != null)
            {
                yield return GetRenamePrimaryKeyScript(tableName, primaryKeyDifference);
            }

            if (primaryKeyDifference.DescriptionChangedTo != null)
            {
                yield return descriptionScriptBuilder.ChangePrimaryKeyDescription(
                    tableName,
                    primaryKeyDifference.CurrentPrimaryKey.SqlName,
                    primaryKeyDifference.CurrentPrimaryKey.Description,
                    primaryKeyDifference.RequiredPrimaryKey.Description);
            }
        }

        private IEnumerable<IScriptTask> GetAddPrimaryKeyScripts(ObjectName tableName, PrimaryKeyDifference primaryKeyDifference)
        {
            var requiredPrimaryKey = primaryKeyDifference.RequiredPrimaryKey;
            var primaryKeyName = requiredPrimaryKey?.Name ?? GetNameForPrimaryKey(tableName, primaryKeyDifference.RequiredPrimaryKey, primaryKeyDifference.RequiredColumns);
            var clusteredClause = requiredPrimaryKey?.Clustered == true
                ? ""
                : " NONCLUSTERED";

            yield return new SqlScript($@"ALTER TABLE {tableName.SqlSafeName()}
ADD CONSTRAINT {primaryKeyName.SqlSafeName()}
PRIMARY KEY{clusteredClause} ({string.Join(", ", primaryKeyDifference.RequiredColumns.Select(col => col.SqlSafeName()))})
GO")
.Requiring().Columns(tableName.Components(primaryKeyDifference.RequiredColumns)).ToBeCreatedOrAltered()
.Requiring().PrimaryKeyOn(tableName).ToBeDropped()
.CreatesOrAlters().PrimaryKeyOn(tableName);

            if (requiredPrimaryKey?.Description != null)
            {
                yield return descriptionScriptBuilder.ChangePrimaryKeyDescription(
                    tableName,
                    primaryKeyName,
                    null,
                    requiredPrimaryKey?.Description);
            }
        }

        private string GetNameForPrimaryKey(
            ObjectName tableName,
            PrimaryKeyDetails primaryKeyDetails,
            string[] requiredColumns)
        {
            if (!string.IsNullOrEmpty(primaryKeyDetails?.Name))
                return primaryKeyDetails.Name;

            var hashOfRequiredColumns = hashHelper.GetHash(string.Join(",", requiredColumns), 8);

            return objectNameHelper.GetAutomaticConstraintName(
                "PK",
                tableName.Name,
                null,
                PrimaryKeyObjectIdPrefix + hashOfRequiredColumns);
        }

        private IScriptTask GetDropPrimaryKeyScript(ObjectName tableName, PrimaryKeyDetails currentPrimaryKey)
        {
            return new SqlScript($@"ALTER TABLE {tableName.SqlSafeName()}
DROP CONSTRAINT {currentPrimaryKey.SqlName.SqlSafeName()}
GO").Drops().PrimaryKeyOn(tableName)
    .Requiring().ForeignKeyReferencing(tableName).ToBeDropped();
        }

        private IScriptTask GetRenamePrimaryKeyScript(ObjectName tableName, PrimaryKeyDifference primaryKeyDifference)
        {
            if (string.IsNullOrEmpty(primaryKeyDifference.RequiredPrimaryKey?.Name))
            {
                var newName = GetNameForPrimaryKey(tableName, primaryKeyDifference.RequiredPrimaryKey, primaryKeyDifference.RequiredColumns);

                return new SqlScript(@$"EXEC sp_rename 
    @objname = '{tableName.Schema}.{primaryKeyDifference.CurrentPrimaryKey.SqlName}', 
    @newname = '{newName}', 
    @objtype = 'OBJECT'
GO").CreatesOrAlters().PrimaryKeyOn(tableName);
            }

            return new SqlScript($@"EXEC sp_rename 
    @objname = '{primaryKeyDifference.CurrentPrimaryKey.SqlName}', 
    @newname = '{primaryKeyDifference.RequiredPrimaryKey.Name}', 
    @objtype = 'OBJECT'
GO").CreatesOrAlters().PrimaryKeyOn(tableName);
        }
    }
}
