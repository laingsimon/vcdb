using System.Collections.Generic;
using System.Linq;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;
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

        public IEnumerable<SqlScript> CreateUpgradeScripts(TableName tableName, PrimaryKeyDifference primaryKeyDifference)
        {
            if (primaryKeyDifference == null)
            {
                yield break;
            }

            if (primaryKeyDifference.Added)
            {
                foreach (var script in GetAddPrimaryKeyScripts(tableName, primaryKeyDifference))
                {
                    yield return script;
                }
                yield break;
            }

            if (primaryKeyDifference.Removed)
            {
                foreach (var script in GetDropPrimaryKeyScripts(tableName, primaryKeyDifference.CurrentPrimaryKey))
                {
                    yield return script;
                }
                yield break;
            }

            if (primaryKeyDifference.ClusteredChangedTo != null || primaryKeyDifference.ColumnsAdded.Any() || primaryKeyDifference.ColumnsRemoved.Any())
            {
                foreach (var script in GetDropPrimaryKeyScripts(tableName, primaryKeyDifference.CurrentPrimaryKey))
                {
                    yield return script;
                }
                foreach (var script in GetAddPrimaryKeyScripts(tableName, primaryKeyDifference))
                {
                    yield return script;
                }
                yield break;
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

        private IEnumerable<SqlScript> GetAddPrimaryKeyScripts(TableName tableName, PrimaryKeyDifference primaryKeyDifference)
        {
            var requiredPrimaryKey = primaryKeyDifference.RequiredPrimaryKey;
            var primaryKeyName = requiredPrimaryKey?.Name ?? GetNameForPrimaryKey(tableName, primaryKeyDifference.RequiredPrimaryKey, primaryKeyDifference.RequiredColumns);
            var clusteredClause = (requiredPrimaryKey?.Clustered ?? true)
                ? ""
                : " NONCLUSTERED";

            yield return new SqlScript($@"ALTER TABLE {tableName.SqlSafeName()}
ADD CONSTRAINT {primaryKeyName.SqlSafeName()}
PRIMARY KEY{clusteredClause} ({string.Join(", ", primaryKeyDifference.RequiredColumns.Select(col => col.SqlSafeName()))})
GO");

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
            TableName tableName,
            PrimaryKeyDetails primaryKeyDetails,
            string[] requiredColumns)
        {
            if (!string.IsNullOrEmpty(primaryKeyDetails?.Name))
                return primaryKeyDetails.Name;

            var hashOfRequiredColumns = hashHelper.GetHash(string.Join(",", requiredColumns), 8);

            return objectNameHelper.GetAutomaticConstraintName(
                "PK",
                tableName.Table,
                null,
                PrimaryKeyObjectIdPrefix + hashOfRequiredColumns);
        }

        private IEnumerable<SqlScript> GetDropPrimaryKeyScripts(TableName tableName, PrimaryKeyDetails currentPrimaryKey)
        {
            yield return new SqlScript($@"ALTER TABLE {tableName.SqlSafeName()}
DROP CONSTRAINT {currentPrimaryKey.SqlName.SqlSafeName()}
GO");
        }

        private SqlScript GetRenamePrimaryKeyScript(TableName tableName, PrimaryKeyDifference primaryKeyDifference)
        {
            if (string.IsNullOrEmpty(primaryKeyDifference.RequiredPrimaryKey?.Name))
            {
                var newName = GetNameForPrimaryKey(tableName, primaryKeyDifference.RequiredPrimaryKey, primaryKeyDifference.RequiredColumns);

                return new SqlScript(@$"EXEC sp_rename 
    @objname = '{tableName.Schema}.{primaryKeyDifference.CurrentPrimaryKey.SqlName}', 
    @newname = '{newName}', 
    @objtype = 'OBJECT'
GO");
            }

            return new SqlScript($@"EXEC sp_rename 
    @objname = '{primaryKeyDifference.CurrentPrimaryKey.SqlName}', 
    @newname = '{primaryKeyDifference.RequiredPrimaryKey.Name}', 
    @objtype = 'OBJECT'
GO");
        }
    }
}
