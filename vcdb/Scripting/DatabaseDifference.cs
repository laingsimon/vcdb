using System.Collections.Generic;

namespace vcdb.Scripting
{
    public class DatabaseDifference
    {
        public const string UnchangedDescription = "\0";

        public IReadOnlyCollection<TableDifference> TableDifferences { get; set; }
        public IReadOnlyCollection<SchemaDifference> SchemaDifferences { get; set; }

        public string DescriptionChangedTo { get; set; }

        public bool DescriptionHasChanged
        {
            get
            {
                return DescriptionChangedTo != UnchangedDescription;
            }
        }
    }
}
