using System;
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
        private readonly IDescriptionScriptBuilder descriptionScriptBuilder;
        private readonly ISqlObjectNameHelper objectNameHelper;

        public SqlServerPrimaryKeyScriptBuilder(IDescriptionScriptBuilder descriptionScriptBuilder, ISqlObjectNameHelper objectNameHelper)
        {
            this.descriptionScriptBuilder = descriptionScriptBuilder;
            this.objectNameHelper = objectNameHelper;
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
                foreach (var script in GetRenameIndexScript(tableName, primaryKeyDifference))
                {
                    yield return script;
                }
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

            if (requiredPrimaryKey?.Name == null)
            {
                yield return new SqlScript(@$"DECLARE @newName VARCHAR(1024)
SELECT @newName = 'PK__{tableName.Table}__' + col.name + '__' + FORMAT(k.OBJECT_ID, 'X')
FROM sys.key_constraints k
INNER JOIN sys.index_columns ic
ON ic.object_id = k.parent_object_id
INNER JOIN sys.columns col
ON col.column_id = ic.column_id
AND col.object_id = k.parent_object_id
INNER JOIN sys.tables tab
ON tab.object_id = k.parent_object_id
WHERE tab.name = '{tableName.Table}'
AND SCHEMA_NAME(tab.schema_id) = '{tableName.Schema}'
AND k.name = '{primaryKeyName}'

EXEC sp_rename 
    @objname = '{tableName.Schema}.{primaryKeyName}', 
    @newname = @newName, 
    @objtype = 'OBJECT'
GO");
            }
        }

        private string GetNameForPrimaryKey(
            TableName tableName,
            PrimaryKeyDetails primaryKeyDetails,
            string[] requiredColumns)
        {
            if (!string.IsNullOrEmpty(primaryKeyDetails?.Name))
                return primaryKeyDetails.Name;

            return objectNameHelper.GetAutomaticConstraintName(
                "PK",
                tableName.Table,
                string.Join("__", requiredColumns),
                primaryKeyDetails?.ObjectId ?? 0);
        }

        private IEnumerable<SqlScript> GetDropPrimaryKeyScripts(TableName tableName, PrimaryKeyDetails currentPrimaryKey)
        {
            yield return new SqlScript($@"ALTER TABLE {tableName.SqlSafeName()}
DROP CONSTRAINT {currentPrimaryKey.SqlName.SqlSafeName()}
GO");
        }

        private IEnumerable<SqlScript> GetRenameIndexScript(TableName tableName, PrimaryKeyDifference primaryKeyDifference)
        {
            if (primaryKeyDifference.RequiredPrimaryKey.Name == null)
            {
                yield return new SqlScript(@$"DECLARE @newName VARCHAR(1024)
SELECT @newName = 'PK__{tableName.Table}__' + col.name + '__' + FORMAT(chk.OBJECT_ID, 'X')
FROM sys.check_constraints chk
INNER JOIN sys.columns col
ON col.column_id = chk.parent_column_id
AND col.object_id = chk.parent_object_id
INNER JOIN sys.tables tab
ON tab.object_id = chk.parent_object_id
WHERE tab.name = '{tableName.Table}'
AND SCHEMA_NAME(tab.schema_id) = '{tableName.Schema}'
AND chk.name = '{primaryKeyDifference.CurrentPrimaryKey.SqlName}'

EXEC sp_rename 
    @objname = '{tableName.Schema}.{primaryKeyDifference.CurrentPrimaryKey.SqlName}', 
    @newname = @newName, 
    @objtype = 'OBJECT'
GO");
            }

            yield return new SqlScript($@"EXEC sp_rename 
    @objname = '{primaryKeyDifference.CurrentPrimaryKey.SqlName}', 
    @newname = '{primaryKeyDifference.RequiredPrimaryKey.Name}', 
    @objtype = 'OBJECT'
GO");
        }
    }
}
