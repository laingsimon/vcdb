namespace vcdb.MySql.SchemaBuilding.Models
{
    public class CheckConstraint
    {
        public string constraint_name { get; set; }
        public string constraint_type { get; set; }
        public string enforced { get; set; }
        public string check_clause { get; set; }

        public bool IsEnforced => enforced == "YES";
    }
}
