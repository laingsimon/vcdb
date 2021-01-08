namespace vcdb.SqlServer.SchemaBuilding
{
    public class CheckConstraintDetails : ISqlColumnNamedObject
    {
        public string COLUMN_NAME { get; set; }
        public string CHECK_NAME { get; set; }
        public int OBJECT_ID { get; set; }
        public string DEFINITION { get; set; }

        string ISqlColumnNamedObject.Name => CHECK_NAME;
        int ISqlColumnNamedObject.ObjectId => OBJECT_ID;
        string ISqlColumnNamedObject.ColumnName => COLUMN_NAME;
        string ISqlColumnNamedObject.Prefix => "CK";
    }
}
