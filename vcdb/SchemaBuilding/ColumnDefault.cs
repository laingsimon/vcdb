namespace vcdb.SchemaBuilding
{
    public class ColumnDefault
    {
        public string Name { get; set; }
        public int ObjectId { get; set; }
        public bool IsSystemNamed { get; set; }
        public object Definition { get; set; }
    }
}
