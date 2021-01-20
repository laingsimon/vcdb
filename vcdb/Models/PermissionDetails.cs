namespace vcdb.Models
{
    public class PermissionDetails
    {
        /// <summary>
        /// Whether this permission also allows the grantee the abilty to re-grant permissions
        /// </summary>
        public bool WithGrant { get; set; }
    }
}
