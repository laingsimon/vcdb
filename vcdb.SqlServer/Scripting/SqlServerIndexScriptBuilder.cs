﻿using System.Collections.Generic;
using System.Linq;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;
using vcdb.Scripting.ExecutionPlan;
using vcdb.Scripting.Index;

namespace vcdb.SqlServer.Scripting
{
    public class SqlServerIndexScriptBuilder : IIndexScriptBuilder
    {
        private readonly IDescriptionScriptBuilder descriptionScriptBuilder;

        public SqlServerIndexScriptBuilder(IDescriptionScriptBuilder descriptionScriptBuilder)
        {
            this.descriptionScriptBuilder = descriptionScriptBuilder;
        }

        public IEnumerable<IScriptTask> CreateUpgradeScripts(ObjectName requiredTableName, IReadOnlyCollection<IndexDifference> indexDifferences)
        {
            foreach (var indexDifference in indexDifferences)
            {
                if (indexDifference.IndexAdded)
                {
                    yield return new MultiScriptTask(GetAddIndexScript(requiredTableName, indexDifference.RequiredIndex.Key, indexDifference.RequiredIndex.Value));
                    continue;
                }

                if (indexDifference.IndexDeleted)
                {
                    yield return GetDropIndexScript(requiredTableName, indexDifference.CurrentIndex.Key);
                    continue;
                }

                if (indexDifference.ClusteredChangedTo != null || indexDifference.ChangedColumns.Any() || indexDifference.ChangedIncludedColumns.Any() || indexDifference.UniqueChangedTo != null)
                {
                    //Indexes cannot be altered, they have to be dropped and re-created
                    yield return GetDropIndexScript(requiredTableName, indexDifference.CurrentIndex.Key);
                    yield return new MultiScriptTask(GetAddIndexScript(requiredTableName, indexDifference.RequiredIndex.Key, indexDifference.RequiredIndex.Value));
                }
                else if (indexDifference.IndexRenamedTo != null)
                {
                    yield return GetRenameIndexScript(requiredTableName, indexDifference.CurrentIndex.Key, indexDifference.IndexRenamedTo);
                }

                if (indexDifference.DescriptionChangedTo != null)
                {
                    yield return descriptionScriptBuilder.ChangeIndexDescription(
                        requiredTableName,
                        indexDifference.RequiredIndex.Key,
                        indexDifference.CurrentIndex.Value.Description,
                        indexDifference.DescriptionChangedTo.Value);
                }
            }
        }

        private IScriptTask GetDropIndexScript(ObjectName requiredTableName, string indexName)
        {
            return new SqlScript($@"DROP INDEX {indexName.SqlSafeName()} ON {requiredTableName.SqlSafeName()}
GO").Drops().Index(requiredTableName.Component(indexName));
        }

        private IEnumerable<IScriptTask> GetAddIndexScript(ObjectName tableName, string indexName, IndexDetails index)
        {
            var uniqueClause = index.Unique
                ? "UNIQUE "
                : "";
            var clusteredClause = index.Clustered
                ? "CLUSTERED "
                : "";

            static string IndexColumnSpec(KeyValuePair<string, IndexColumnDetails> col)
            {
                var descendingClause = col.Value.Descending
                    ? " DESC"
                    : "";
                return $"{col.SqlSafeName()}{descendingClause}";
            }

            var columns = string.Join(", ", index.Columns.Select(IndexColumnSpec));
            var includeClause = index.Including?.Any() == true
                ? $"\r\nINCLUDE ({string.Join(", ", index.Including.Select(col => col.SqlSafeName()))})"
                : "";

            yield return new SqlScript($@"CREATE {uniqueClause}{clusteredClause}INDEX {indexName.SqlSafeName()}
ON {tableName.SqlSafeName()} ({columns}){includeClause}
GO")
    .Requiring().Table(tableName).ToBeCreatedOrAltered()
    .CreatesOrAlters().Index(tableName.Component(indexName));

            if (index.Description != null)
            {
                yield return descriptionScriptBuilder.ChangeIndexDescription(
                    tableName,
                    indexName,
                    null,
                    index.Description);
            }
        }

        private IScriptTask GetRenameIndexScript(ObjectName tableName, string currentName, string requiredName)
        {
            return new SqlScript(@$"EXEC sp_rename
    @objname = '{tableName.Schema}.{tableName.Name}.{currentName}',
    @newname = '{requiredName}',
    @objtype = 'INDEX'
GO").CreatesOrAlters().Index(tableName.Component(requiredName));
        }
    }
}
