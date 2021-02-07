namespace vcdb.SqlServer.SchemaBuilding.Models
{
    public class CheckConstraint
    {
        public string COLUMN_NAME { get; set; }
        public string name { get; set; }
        public int object_id { get; set; }
        public string definition { get; set; }
        public bool is_system_named { get; set; }
    }
}
