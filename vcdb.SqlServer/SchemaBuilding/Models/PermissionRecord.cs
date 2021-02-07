using System;
using vcdb.Models;

namespace vcdb.SqlServer.SchemaBuilding.Models
{
    internal class PermissionRecord
    {
        public string class_desc { get; set; }
        public string major_id { get; set; }
        public string object_name { get; set; }
        public string minor_id { get; set; }
        public string minor_name { get; set; }

        [Obsolete("Used for Dapper mapping, do not use directly")]
        public string grantee_principal { get; set; }
        public string grantor_principal { get; set; }

        [Obsolete("Used for Dapper mapping, do not use directly")]
        public string permission_name { get; set; }
        public string state_desc { get; set; }

        public PermissionName PermissionName
        {
#pragma warning disable CS0618 // Type or member is obsolete
            get => new PermissionName(permission_name);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public UserPrincipal GranteePrincipal
        {
#pragma warning disable CS0618 // Type or member is obsolete
            get => new UserPrincipal(grantee_principal);
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
