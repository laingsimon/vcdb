﻿using System.Collections.Generic;
using vcdb.Output;
using vcdb.Scripting.Column;
using vcdb.Scripting.Table;

namespace vcdb.Scripting
{
    public interface IDefaultConstraintScriptBuilder
    {
        IEnumerable<SqlScript> CreateUpgradeScripts(TableDifference tableDifference);
        IEnumerable<SqlScript> CreateUpgradeScripts(ObjectName requiredTableName, ColumnDifference columnDifference);
    }
}