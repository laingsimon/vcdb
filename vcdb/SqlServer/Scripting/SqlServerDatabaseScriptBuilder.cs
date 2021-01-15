﻿using System;
using System.Collections.Generic;
using vcdb.CommandLine;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;
using vcdb.Scripting.Database;
using vcdb.Scripting.Schema;
using vcdb.Scripting.Table;

namespace vcdb.SqlServer.Scripting
{
    public class SqlServerDatabaseScriptBuilder : IDatabaseScriptBuilder
    {
        private readonly Options options;
        private readonly ITableScriptBuilder tableScriptBuilder;
        private readonly ISchemaScriptBuilder schemaScriptBuilder;
        private readonly IDatabaseComparer databaseComparer;
        private readonly IDescriptionScriptBuilder descriptionScriptBuilder;

        public SqlServerDatabaseScriptBuilder(
            Options options,
            ITableScriptBuilder tableScriptBuilder,
            ISchemaScriptBuilder schemaScriptBuilder,
            IDatabaseComparer databaseComparer,
            IDescriptionScriptBuilder descriptionScriptBuilder)
        {
            this.options = options;
            this.tableScriptBuilder = tableScriptBuilder;
            this.schemaScriptBuilder = schemaScriptBuilder;
            this.databaseComparer = databaseComparer;
            this.descriptionScriptBuilder = descriptionScriptBuilder;
        }

        public IEnumerable<SqlScript> CreateUpgradeScripts(DatabaseDetails current, DatabaseDetails required)
        {
            var comparerContext = new ComparerContext();
            var databaseDifferences = databaseComparer.GetDatabaseDifferences(comparerContext, current, required);

            if (databaseDifferences.CollationChangedTo != null)
                yield return GetChangeDatabaseCollationScript(databaseDifferences.CollationChangedTo);

            if (databaseDifferences.DescriptionChangedTo != null)
                yield return descriptionScriptBuilder.ChangeDatabaseDescription(current.Description, databaseDifferences.DescriptionChangedTo.Value);

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
