namespace vcdb.SqlServer.SchemaBuilding
{
    public class SqlServerColumnDefault
    {
        public string column_name { get; set; }

        public string name { get; set; }
        public int object_id { get; set; }
        public bool is_system_named { get; set; }
        public object definition { get; set; }
    }
}
