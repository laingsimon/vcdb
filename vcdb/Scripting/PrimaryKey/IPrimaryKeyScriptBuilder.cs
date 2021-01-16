﻿using System.Collections.Generic;
using vcdb.Output;

namespace vcdb.Scripting.PrimaryKey
{
    public interface IPrimaryKeyScriptBuilder
    {
        IEnumerable<SqlScript> CreateUpgradeScripts(TableName tableName, PrimaryKeyDifference primaryKeyDifference);
    }
}