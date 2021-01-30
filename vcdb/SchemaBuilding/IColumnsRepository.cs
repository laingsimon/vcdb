﻿using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using vcdb.Models;

namespace vcdb.SchemaBuilding
{
    public interface IColumnsRepository
    {
        Task<Dictionary<string, ColumnDetails>> GetColumns(
            DbConnection connection,
            ObjectName tableName,
            Permissions tablePermissions);
    }
}