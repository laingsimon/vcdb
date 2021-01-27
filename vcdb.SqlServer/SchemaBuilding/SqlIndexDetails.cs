namespace vcdb.SqlServer.SchemaBuilding
{
    public class SqlIndexDetails
    {
        public string index_name { get; set; }
        public string type_desc { get; set; }
        public bool is_unique { get; set; }
        public bool is_descending_key { get; set; }
        public bool is_included_column { get; set; }
        public string column_name { get; set; }
    }
}
