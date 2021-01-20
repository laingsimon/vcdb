using System.Collections.Generic;

namespace vcdb.Models
{
    public class PermissionStates
    {
        /// <summary>
        /// The permissions that have been granted to users
        /// </summary>
        public Dictionary<string, Dictionary<string, PermissionDetails>> Grant { get; set; }

        /// <summary>
        /// The users that are denied certain permissions
        /// </summary>
        public Dictionary<string, HashSet<string>> Deny { get; set; }

        /// <summary>
        /// The permissions that have been revoked
        /// This set is only applicable to table-columns
        /// </summary>
        public Dictionary<string, HashSet<string>> Revoke { get; set; }
    }
}
