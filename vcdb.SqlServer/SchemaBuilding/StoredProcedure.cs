namespace vcdb.SqlServer.SchemaBuilding
{
    public class StoredProcedure
    {
        public string name { get; set; }
        public int object_id { get; set; }
        public string schema_name { get; set; }
        public string definition { get; set; }
        public bool is_schema_bound { get; set; }

        internal ObjectName GetName()
        {
            return new ObjectName
            {
                Name = name,
                Schema = schema_name
            };
        }
    }
}
