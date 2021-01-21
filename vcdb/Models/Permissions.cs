using Newtonsoft.Json;
using System.Collections.Generic;

namespace vcdb.Models
{
    public class Permissions
    {
        /// <summary>
        /// A mapping of permission-type -> username -> details
        /// </summary>
        public Dictionary<PermissionName, Dictionary<UserPrincipal, PermissionDetails>> Grant { get; set; }

        /// <summary>
        /// A mapping of permission-type -> username -> details
        /// </summary>
        public Dictionary<PermissionName, HashSet<UserPrincipal>> Deny { get; set; }

        /// <summary>
        /// A mapping of permission-type -> username -> details
        /// </summary>
        public Dictionary<PermissionName, HashSet<UserPrincipal>> Revoke { get; set; }

        /// <summary>
        /// Permissions for sub-entities within this entity, for example columns within a table
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, Permissions> SubEntityPermissions { get; set; }
    }
}
