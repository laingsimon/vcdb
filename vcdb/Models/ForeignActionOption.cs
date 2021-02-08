namespace vcdb.Models
{
    /// <summary>
    /// Represents the action that should be performed on a record in a referencing table should the referenced record be updated or deleted
    /// 
    /// SqlServer: https://docs.microsoft.com/en-us/sql/t-sql/statements/alter-table-table-constraint-transact-sql?view=sql-server-ver15
    /// </summary>
    public enum ForeignActionOption
    {
        /// <summary>
        /// The Database Engine raises an error, and the update action on the row in the parent table is rolled back.
        /// </summary>
        NoAction,

        /// <summary>
        /// Corresponding rows are updated in the referencing table when that row is updated in the parent table.
        /// </summary>
        Cascade,

        /// <summary>
        /// All the values that make up the foreign key are set to NULL when the corresponding row in the parent table is updated. For this constraint to execute, the foreign key columns must be nullable.
        /// </summary>
        SetNull,

        /// <summary>
        /// All the values that make up the foreign key are set to their default values when the corresponding row in the parent table is updated. For this constraint to execute, all foreign key columns must have default definitions. If a column is nullable, and there is no explicit default value set, NULL becomes the implicit default value of the column.
        /// </summary>
        SetDefault
    }
}
