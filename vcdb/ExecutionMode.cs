namespace vcdb
{
    public enum ExecutionMode
    {
        /// <summary>
        /// Produce a representation of the database in JSON format
        /// </summary>
        Construct,
        /// <summary>
        /// Produce a SQL script to get the database to the same state as that specified in the input JSON representation
        /// </summary>
        Differential
    }
}
