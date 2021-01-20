namespace vcdb.SqlServer.SchemaBuilding
{
    internal class PermissionRecord
    {
        public string class_desc { get; set; }
        public string major_id { get; set; }
        public string major_name { get; set; }
        public string minor_id { get; set; }
        public string minor_name { get; set; }
        public string grantee_principal { get; set; }
        public string grantor_principal { get; set; }
        public string permission_name { get; set; }
        public string state_desc { get; set; }
    }
}
