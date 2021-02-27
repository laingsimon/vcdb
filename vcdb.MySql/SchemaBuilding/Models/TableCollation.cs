namespace vcdb.MySql.SchemaBuilding.Models
{
    public class TableCollation
    {
        public string column_name { get; set; }
        public string charset_name { get; set; }
        public string collation_name { get; set; }
    }
}
