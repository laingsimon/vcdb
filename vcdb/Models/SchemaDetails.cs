namespace vcdb.Models
{
    public class SchemaDetails : INamedItem<string>
    {
        /// <summary>
        /// Any previous names for the schema, to indicate whether the schema might need to change name
        /// </summary>
        public string[] PreviousNames { get; set; }

        /// <summary>
        /// A description for the schema
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A record of permissions for this schema
        /// </summary>
        public Permissions Permissions { get; set; }
    }
}
