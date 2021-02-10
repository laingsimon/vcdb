namespace vcdb.Scripting.ExecutionPlan
{
    public enum DependencyAction
    {
        /// <summary>
        /// The script results in the identified object being dropped
        /// </summary>
        Drop,

        /// <summary>
        /// The script results in the identified object being created, altered or renamed
        /// </summary>
        CreateOrAlter
    }
}
