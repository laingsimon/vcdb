namespace vcdb.Models
{
    public enum UserType
    { 
        /// <summary>
        /// The user is authorised by the database server
        /// </summary>
        DatabaseAuthority,

        /// <summary>
        /// The user is authorised by a windows domain controller
        /// </summary>
        WindowsAuthority,

        /// <summary>
        /// The user is authorised by a certificate
        /// </summary>
        CertificateAuthority,

        /// <summary>
        /// The user is authorised by an asymmetric key
        /// </summary>
        KeyAuthority
    }
}
