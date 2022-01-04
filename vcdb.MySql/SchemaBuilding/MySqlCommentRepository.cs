using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using vcdb.CommandLine;
using vcdb.SchemaBuilding;

namespace vcdb.MySql.SchemaBuilding
{
    public class MySqlCommentRepository : IDescriptionRepository
    {
        private readonly Options options;

        public MySqlCommentRepository(Options options)
        {
            this.options = options;
        }

        public async Task<Dictionary<string, string>> GetColumnDescriptions(DbConnection connection, ObjectName tableName)
        {
            var columnComments = await connection.QueryAsync<CommentMapping>(@"
select COLUMN_NAME as Name, COLUMN_COMMENT as Comment
from INFORMATION_SCHEMA.COLUMNS
where TABLE_SCHEMA = @databaseName
and TABLE_NAME = @tableName", new { databaseName = options.Database, tableName = tableName.Name });
            return columnComments
                .Where(mapping => !string.IsNullOrEmpty(mapping.Comment))
                .ToDictionary(
                    mapping => mapping.Name,
                    mapping => mapping.Comment);
        }

        public async Task<Dictionary<string, string>> GetForeignKeyDescription(DbConnection connection, ObjectName tableName)
        {
            var indexComments = await connection.QueryAsync<CommentMapping>(@"
select COLUMN_NAME as Name, INDEX_COMMENT as Comment
from INFORMATION_SCHEMA.STATISTICS
where TABLE_SCHEMA = @databaseName
and INDEX_NAME = 'PRIMARY'
and TABLE_NAME = @tableName", new { databaseName = options.Database, tableName = tableName.Name });
            return indexComments
                .Where(mapping => !string.IsNullOrEmpty(mapping.Comment))
                .ToDictionary(
                    mapping => mapping.Name,
                    mapping => mapping.Comment);
        }

        public async Task<Dictionary<string, string>> GetIndexDescriptions(DbConnection connection, ObjectName tableName)
        {
            var indexComments = await connection.QueryAsync<CommentMapping>(@"
select INDEX_NAME as Name, INDEX_COMMENT as Comment
from INFORMATION_SCHEMA.STATISTICS
where TABLE_SCHEMA = @databaseName
and INDEX_NAME <> 'PRIMARY'
and TABLE_NAME = @tableName", new { databaseName = options.Database, tableName = tableName.Name });
            return indexComments
                .Where(mapping => !string.IsNullOrEmpty(mapping.Comment))
                .ToDictionary(
                    mapping => mapping.Name,
                    mapping => mapping.Comment);
        }

        public Task<string> GetPrimaryKeyDescription(DbConnection connection, ObjectName tableName, string primaryKeyName)
        {
            return Task.FromResult<string>(null);
        }

        public async Task<string> GetProcedureDescription(DbConnection connection, ObjectName procedureName)
        {
            var routineComment = await connection.QuerySingleOrDefaultAsync<string>(@"
select ROUTINE_COMMENT
from INFORMATION_SCHEMA.ROUTINES
where ROUTINE_SCHEMA = @databaseName
and ROUTINE_NAME = @routineName", new { databaseName = options.Database, routineName = procedureName.Name });
            return string.IsNullOrEmpty(routineComment)
                ? null
                : routineComment;
        }

        public async Task<string> GetTableDescription(DbConnection connection, ObjectName tableName)
        {
            var tableComment = await connection.QuerySingleOrDefaultAsync<string>($@"
select TABLE_COMMENT
from INFORMATION_SCHEMA.TABLES
where TABLE_TYPE = 'BASE TABLE'
and TABLE_SCHEMA = @databaseName
and TABLE_NAME = @tableName", new { databaseName = options.Database, tableName = tableName.Name });
            return string.IsNullOrEmpty(tableComment)
                ? null
                : tableComment;
        }

        #region not supported
        public Task<string> GetDatabaseDescription(DbConnection connection)
        {
            return Task.FromResult<string>(null);
        }

        public Task<string> GetSchemaDescription(DbConnection connection, string schemaName)
        {
            return Task.FromResult<string>(null);
        }
        #endregion

        // ReSharper disable once ClassNeverInstantiated.Local
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local", Justification = "Dapper sets these properties, as such they're not identified as ever being set")]
        private class CommentMapping
        {
            public string Name { get; set; }
            public string Comment { get; set; }
        }
    }
}
