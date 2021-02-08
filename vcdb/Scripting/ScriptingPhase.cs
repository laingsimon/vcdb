namespace vcdb.Scripting
{
    public enum ScriptingPhase
    {
        /// <summary>
        /// Drop any objects that reference other dependencies, e.g. drop and foreign keys before their primary keys can be dropped
        /// </summary>
        DropReferences,

        /// <summary>
        /// Drop any objects that would hinder any changes being made the the source object.
        /// e.g. Drop any foreign key constraints before they are renamed (<see cref="Alter"/> phase)
        /// </summary>
        DropDependencies,

        /// <summary>
        /// Apply any alterations (renames, retyping, etc.) that must happen before objects can be recreated - possibly referencing these altered objects
        /// e.g. Rename a column before a foreign key can be (re) created (<see cref="Recreate"/> phase).
        /// </summary>
        Alter,

        /// <summary>
        /// Create any items that can be dependencies on other script components, e.g. create PrimaryKeys that are required later by a ForeignKey constraint
        /// </summary>
        RecreateDependencies,

        /// <summary>
        /// (Re)create any objects as required, all dependencies should have been altered prior to this phase executing
        /// </summary>
        Recreate
    }
}
