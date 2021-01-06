using vcdb.Models;

namespace vcdb.Scripting
{
    public class SchemaDifference
    {
        public const string UnchangedDescription = "\0";

        public NamedItem<string, SchemaDetails> RequiredSchema { get; set; }
        public NamedItem<string, SchemaDetails> CurrentSchema { get; set; }
        public bool SchemaAdded { get; set; }
        public bool SchemaDeleted { get; set; }
        public string SchemaRenamedTo { get; set; }
        public string DescriptionChangedTo { get; set; } = UnchangedDescription;

        public bool DescriptionHasChanged
        {
            get
            {
                return DescriptionChangedTo != UnchangedDescription;
            }
        }

        public bool IsChanged
        {
            get
            {
                return SchemaRenamedTo != null
                    || SchemaAdded
                    || SchemaDeleted
                    || DescriptionHasChanged;
            }
        }
    }
}
