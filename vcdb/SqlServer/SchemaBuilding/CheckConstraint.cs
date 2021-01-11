namespace vcdb.SqlServer.SchemaBuilding
{
    public class CheckConstraint
    {
        public string column_name { get; set; }
        public string name { get; set; }
        public int object_id { get; set; }
        public string definition { get; set; }
        public bool is_system_named { get; set; }
    }
}
