using System.Collections.Generic;
using System.Linq;
using vcdb.CommandLine;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;
using vcdb.Scripting.Database;
using vcdb.Scripting.Permission;
using vcdb.Scripting.Programmability;
using vcdb.Scripting.Schema;
using vcdb.Scripting.Table;
using vcdb.Scripting.User;

namespace vcdb.SqlServer.Scripting
{
    public class SqlServerDatabaseScriptBuilder : IDatabaseScriptBuilder
    {
        private readonly Options options;
        private readonly ITableScriptBuilder tableScriptBuilder;
        private readonly ISchemaScriptBuilder schemaScriptBuilder;
        private readonly IDatabaseComparer databaseComparer;
        private readonly IDescriptionScriptBuilder descriptionScriptBuilder;
        private readonly IUserScriptBuilder userScriptBuilder;
        private readonly IPermissionScriptBuilder permissionScriptBuilder;
        private readonly IProcedureScriptBuilder procedureScriptBuilder;

        public SqlServerDatabaseScriptBuilder(
            Options options,
            ITableScriptBuilder tableScriptBuilder,
            ISchemaScriptBuilder schemaScriptBuilder,
            IDatabaseComparer databaseComparer,
            IDescriptionScriptBuilder descriptionScriptBuilder,
            IUserScriptBuilder userScriptBuilder,
            IPermissionScriptBuilder permissionScriptBuilder,
            IProcedureScriptBuilder procedureScriptBuilder)
        {
            this.options = options;
            this.tableScriptBuilder = tableScriptBuilder;
            this.schemaScriptBuilder = schemaScriptBuilder;
            this.databaseComparer = databaseComparer;
            this.descriptionScriptBuilder = descriptionScriptBuilder;
            this.userScriptBuilder = userScriptBuilder;
            this.permissionScriptBuilder = permissionScriptBuilder;
            this.procedureScriptBuilder = procedureScriptBuilder;
        }

        public IEnumerable<IOutputable> CreateUpgradeScripts(DatabaseDetails current, DatabaseDetails required)
        {
            var comparerContext = new ComparerContext();
            var databaseDifferences = databaseComparer.GetDatabaseDifferences(comparerContext, current, required);

            if (databaseDifferences.CollationChangedTo != null)
                yield return GetChangeDatabaseCollationScript(databaseDifferences.CollationChangedTo);

            if (databaseDifferences.DescriptionChangedTo != null)
                yield return descriptionScriptBuilder.ChangeDatabaseDescription(current.Description, databaseDifferences.DescriptionChangedTo.Value);

            yield return new OutputableCollection(userScriptBuilder.CreateUpgradeScripts(databaseDifferences.UserDifferences));
            yield return new OutputableCollection(permissionScriptBuilder.CreateDatabasePermissionScripts(databaseDifferences.PermissionDifferences));

            var schemaObjectDifferences = databaseDifferences.TableDifferences.Cast<ISchemaObjectDifference>()
                .Concat(databaseDifferences.ProcedureDifferences.Cast<ISchemaObjectDifference>())
                .ToArray();
            yield return new OutputableCollection(schemaScriptBuilder.CreateUpgradeScripts(databaseDifferences.SchemaDifferences, schemaObjectDifferences));
            yield return new OutputableCollection(tableScriptBuilder.CreateUpgradeScripts(databaseDifferences.TableDifferences));
            yield return new OutputableCollection(procedureScriptBuilder.CreateUpgradeScripts(databaseDifferences.ProcedureDifferences));
        }

        private SqlScript GetChangeDatabaseCollationScript(string requiredCollation)
        {
            return new SqlScript($@"ALTER DATABASE {options.Database.SqlSafeName()}
COLLATE {requiredCollation}
GO");
        }
    }
}
