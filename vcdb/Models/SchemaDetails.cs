namespace vcdb.Models
{
    public class SchemaDetails : INamedItem<string>
    {
        public string[] PreviousNames { get; set; }

        /// <summary>
        /// A description for the schema
        /// </summary>
        public string Description { get; set; }
    }
}
