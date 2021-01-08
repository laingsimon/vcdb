﻿namespace vcdb.SqlServer.SchemaBuilding
{
    public class ColumnDefaultDetails : ISqlColumnNamedObject
    {
        public string COLUMN_NAME { get; set; }
        public string DEFAULT_NAME { get; set; }
        public int OBJECT_ID { get; set; }

        string ISqlColumnNamedObject.Name => DEFAULT_NAME;
        int ISqlColumnNamedObject.ObjectId => OBJECT_ID;
        string ISqlColumnNamedObject.ColumnName => COLUMN_NAME;
        string ISqlColumnNamedObject.Prefix => "DF";
    }
}
