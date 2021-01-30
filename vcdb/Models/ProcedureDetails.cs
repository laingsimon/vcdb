namespace vcdb.Models
{
    /// <summary>
    /// Represents a procedure that can be executed within the database
    /// </summary>
    public class ProcedureDetails : INamedItem<string>
    {
        public string[] PreviousNames { get; set; }

        /// <summary>
        /// The definition of the stored procedure
        /// </summary>
        public string Definition { get; set; }

        /// <summary>
        /// The name of the file that contains the definition of the stored procedure
        /// </summary>
        public string FileDefinition { get; set; }

        /// <summary>
        /// The permissions bound to this procedure
        /// </summary>
        public Permissions Permissions { get; set; }

        /// <summary>
        /// Any description applied to this procecure
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Whether this procedure should be schema bound or not
        /// </summary>
        public bool SchemaBound { get; set; }

        /// <summary>
        /// Whether the definition of this procedure should be encrypted within the database
        /// </summary>
        public bool Encrypted { get; set; }
    }
}
