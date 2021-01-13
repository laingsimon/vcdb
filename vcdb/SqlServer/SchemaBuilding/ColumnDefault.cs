using vcdb.SchemaBuilding;

namespace vcdb.SqlServer.SchemaBuilding
{
    public class ColumnDefault : IColumnDefault
    {
        public string column_name { get; set; }

        public string Name { get; set; }
        public int ObjectId { get; set; }
        public bool IsSystemNamed { get; set; }
    }
}
