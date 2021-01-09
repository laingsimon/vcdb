namespace vcdb.SqlServer.SchemaBuilding
{
    public class SqlServerCheckConstraintDetails
    {
        public string COLUMN_NAME { get; set; }
        public string CHECK_NAME { get; set; }
        public int OBJECT_ID { get; set; }
        public string DEFINITION { get; set; }
    }
}
