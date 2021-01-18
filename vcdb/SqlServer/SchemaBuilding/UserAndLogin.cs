namespace vcdb.SqlServer.SchemaBuilding
{
    internal class UserAndLogin
    {
        public string name { get; set; }
        public string type_desc { get; set; }
        public string authentication_type_desc { get; set; }
        public string login { get; set; }
        public bool is_disabled { get; set; }
    }
}
