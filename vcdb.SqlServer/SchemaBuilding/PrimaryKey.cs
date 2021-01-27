namespace vcdb.SqlServer.SchemaBuilding
{
    public class PrimaryKey
    {
        public string name { get; set; }
        public bool is_system_named { get; set; }
        public int object_id { get; set; }
        public string type_desc { get; set; }
        public bool is_unique { get; set; }
    }
}
