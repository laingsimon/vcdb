namespace vcdb.SqlServer.SchemaBuilding
{
    public class SpColumnsOutput
    {
        public string TABLE_QUALIFIER { get; set; }
        public string TABLE_OWNER { get; set; }
        public string TABLE_NAME { get; set; }
        public string COLUMN_NAME { get; set; }
        public int DATA_TYPE { get; set; }
        public string TYPE_NAME { get; set; }
        public int PRECISION { get; set; }
        public int LENGTH { get; set; }
        public int? SCALE { get; set; }
        public bool NULLABLE { get; set; }
        public string COLUMN_DEF { get; set; }
        public int ORDINAL_POSITION { get; set; }
        public int? CHAR_OCTET_LENGTH { get; set; }
    }
}
