﻿using System;
using System.Collections.Generic;
using vcdb.CommandLine;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;
using vcdb.Scripting.Database;
using vcdb.Scripting.Permission;
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

        public SqlServerDatabaseScriptBuilder(
            Options options,
            ITableScriptBuilder tableScriptBuilder,
            ISchemaScriptBuilder schemaScriptBuilder,
            IDatabaseComparer databaseComparer,
            IDescriptionScriptBuilder descriptionScriptBuilder,
            IUserScriptBuilder userScriptBuilder,
            IPermissionScriptBuilder permissionScriptBuilder)
        {
            this.options = options;
            this.tableScriptBuilder = tableScriptBuilder;
            this.schemaScriptBuilder = schemaScriptBuilder;
            this.databaseComparer = databaseComparer;
            this.descriptionScriptBuilder = descriptionScriptBuilder;
            this.userScriptBuilder = userScriptBuilder;
            this.permissionScriptBuilder = permissionScriptBuilder;
        }

        public IEnumerable<SqlScript> CreateUpgradeScripts(DatabaseDetails current, DatabaseDetails required)
        {
            var comparerContext = new ComparerContext();
            var databaseDifferences = databaseComparer.GetDatabaseDifferences(comparerContext, current, required);

            if (databaseDifferences.CollationChangedTo != null)
                yield return GetChangeDatabaseCollationScript(databaseDifferences.CollationChangedTo);

            if (databaseDifferences.DescriptionChangedTo != null)
                yield return descriptionScriptBuilder.ChangeDatabaseDescription(current.Description, databaseDifferences.DescriptionChangedTo.Value);

            foreach (var script in userScriptBuilder.CreateUpgradeScripts(databaseDifferences.UserDifferences))
            {
                yield return script;
            }

            foreach (var script in permissionScriptBuilder.CreateDatabasePermissionScripts(databaseDifferences.PermissionDifferences))
            {
                yield return script;
            }

            foreach (var schemaScript in schemaScriptBuilder.CreateUpgradeScripts(databaseDifferences.SchemaDifferences, databaseDifferences.TableDifferences))
            {
                yield return schemaScript;
            }

            foreach (var tableScript in tableScriptBuilder.CreateUpgradeScripts(databaseDifferences.TableDifferences))
            {
                yield return tableScript;
            }
        }

        private SqlScript GetChangeDatabaseCollationScript(string requiredCollation)
        {
            return new SqlScript($@"ALTER DATABASE {options.Database.SqlSafeName()}
COLLATE {requiredCollation}
GO");
        }
    }
}