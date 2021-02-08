namespace vcdb.SqlServer.SchemaBuilding.Models
{
    public class ForeignKey
    {
        public int object_id { get; set; }
        public string name { get; set; }
        public string delete_referential_action_desc { get; set; }
        public string update_referential_action_desc { get; set; }
        public bool is_system_named { get; set; }
        public string source_column { get; set; }
        public string referenced_table_schema { get; set; }
        public string referenced_table { get; set; }
        public string referenced_column { get; set; }
    }
}
