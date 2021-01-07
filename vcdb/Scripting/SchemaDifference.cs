using vcdb.Models;

namespace vcdb.Scripting
{
    public class SchemaDifference
    {
        public NamedItem<string, SchemaDetails> RequiredSchema { get; set; }
        public NamedItem<string, SchemaDetails> CurrentSchema { get; set; }
        public bool SchemaAdded { get; set; }
        public bool SchemaDeleted { get; set; }
        public string SchemaRenamedTo { get; set; }
        public Change<string> DescriptionChangedTo { get; set; }

        public bool IsChanged
        {
            get
            {
                return SchemaRenamedTo != null
                    || SchemaAdded
                    || SchemaDeleted
                    || DescriptionChangedTo != null;
            }
        }
    }
}
