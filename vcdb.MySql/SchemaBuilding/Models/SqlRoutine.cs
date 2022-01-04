namespace vcdb.MySql.SchemaBuilding.Models
{
    public class SqlRoutine
    {
        public string ROUTINE_NAME { get; set; }
        public string ROUTINE_DEFINITION { get; set; }

        public ObjectName GetObjectName()
        {
            return new ObjectName
            {
                Name = ROUTINE_NAME
            };
        }
    }
}
