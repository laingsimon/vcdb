using System.Collections.Generic;
using System.Linq;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;
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

        private SqlScript GetDropIndexScript(TableName requiredTableName, string indexName)
        {
            return new SqlScript($@"DROP INDEX {indexName.SqlSafeName()} ON {requiredTableName.SqlSafeName()}
GO");
        }

        public IEnumerable<SqlScript> CreateUpgradeScripts(TableName requiredTableName, IReadOnlyCollection<IndexDifference> indexDifferences)
        {
            foreach (var indexDifference in indexDifferences)
            {
                if (indexDifference.IndexAdded)
                {
                    foreach (var script in GetAddIndexScript(requiredTableName, indexDifference.RequiredIndex.Key, indexDifference.RequiredIndex.Value))
                    {
                        yield return script;
                    }
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
                    foreach (var script in GetAddIndexScript(requiredTableName, indexDifference.RequiredIndex.Key, indexDifference.RequiredIndex.Value))
                    {
                        yield return script;
                    }
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

        private IEnumerable<SqlScript> GetAddIndexScript(TableName tableName, string indexName, IndexDetails index)
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
GO");

            if (index.Description != null)
            {
                yield return descriptionScriptBuilder.ChangeIndexDescription(
                    tableName,
                    indexName,
                    null,
                    index.Description);
            }
        }

        private SqlScript GetRenameIndexScript(TableName tableName, string currentName, string requiredName)
        {
            return new SqlScript(@$"EXEC sp_rename
    @objname = '{tableName.Schema}.{tableName.Table}.{currentName}',
    @newname = '{requiredName}',
    @objtype = 'INDEX'
GO");
        }
    }
}
