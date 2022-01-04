using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using vcdb.CommandLine;
using vcdb.Models;
using vcdb.MySql.SchemaBuilding.Models;
using vcdb.SchemaBuilding;

namespace vcdb.MySql.SchemaBuilding
{
    public class MySqlProcedureRepository : IProcedureRepository
    {
        private const string RoutineTypeName = "PROCEDURE";

        private readonly Options options;
        private readonly IDescriptionRepository descriptionRepository;

        public MySqlProcedureRepository(
            Options options,
            IDescriptionRepository descriptionRepository)
        {
            this.options = options;
            this.descriptionRepository = descriptionRepository;
        }

        public async Task<Dictionary<ObjectName, ProcedureDetails>> GetProcedures(DbConnection connection)
        {
            var routines = await connection.QueryAsync<SqlRoutine>($@"
select ROUTINE_NAME, ROUTINE_DEFINITION
from information_schema.routines
where routine_type = '{RoutineTypeName}'
and ROUTINE_SCHEMA = @databaseName", new { databaseName = options.Database });

            var parameters = (await connection.QueryAsync<SqlRoutineParameter>($@"
select SPECIFIC_NAME, PARAMETER_MODE, PARAMETER_NAME, DATA_TYPE
from information_schema.parameters
where routine_type = '{RoutineTypeName}'
and SPECIFIC_SCHEMA = @databaseName", new { databaseName = options.Database }))
            .ToLookup(parameter => parameter.SPECIFIC_NAME);

            return await routines.ToDictionaryAsync(
                routine => routine.GetObjectName(),
                async routine =>
                {
                    var routineParameters = parameters[routine.ROUTINE_NAME].ToArray();
                    var comment = await descriptionRepository.GetProcedureDescription(connection, routine.GetObjectName());

                    return new ProcedureDetails
                    {
                        Definition = BuildCreateProcedureStatement(routine, routineParameters, comment),
                        Description = comment,
                    };
                });
        }

        private static string BuildCreateProcedureStatement(SqlRoutine routine, IReadOnlyCollection<SqlRoutineParameter> parameters, string comment)
        {
            var parametersClause = parameters.Any()
                ? $"({string.Join(", ", parameters.Select(BuildParameterStatement))})\r\n"
                : "";

            return $"CREATE {RoutineTypeName} `{routine.ROUTINE_NAME}`\r\n{parametersClause}{BuildCommentClause(comment)}{routine.ROUTINE_DEFINITION}";
        }

        private static string BuildCommentClause(string comment)
        {
            return string.IsNullOrEmpty(comment)
                ? ""
                : $"COMMENT '{comment}'\r\n";
        }

        private static string BuildParameterStatement(SqlRoutineParameter parameter)
        {
            return $"{parameter.PARAMETER_NAME} {parameter.DATA_TYPE}";
        }
    }
}
