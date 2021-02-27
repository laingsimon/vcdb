namespace vcdb.MySql.SchemaBuilding.Models
{
    public class ComputedColumn
    {
        public string TABLE_SCHEMA { get; set; }
        public string TABLE_NAME { get; set; }
        public string COLUMN_NAME { get; set; }
        public string GENERATION_EXPRESSION { get; set; }
    }
}
