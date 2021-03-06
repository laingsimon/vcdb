﻿namespace vcdb.SqlServer
{
    public interface ISqlObjectNameHelper
    {
        string GetAutomaticConstraintName(string constraintPrefix, string tableName, string columnName, int objectId);
        string GetAutomaticConstraintName(string constraintPrefix, string tableName, string columnName, string objectIdSubstitute);
    }
}