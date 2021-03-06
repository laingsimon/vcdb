﻿using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using vcdb.Models;

namespace vcdb.SchemaBuilding
{
    public interface ITableRepository
    {
        Task<Dictionary<ObjectName, TableDetails>> GetTables(DbConnection connection);
    }
}