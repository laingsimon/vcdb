namespace vcdb.MySql.SchemaBuilding.Models
{
    public class SqlIndexDetails
    {
        public string index_name { get; set; }
        public string type_desc { get; set; }
        public bool is_unique { get; set; }
        public string column_name { get; set; }
    }
}
