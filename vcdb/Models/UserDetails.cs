namespace vcdb.Models
{
    public class UserDetails : INamedItem<string>
    {
        /// <summary>
        /// The type of login
        /// </summary>
        public UserType Type { get; set; }

        /// <summary>
        /// Whether this login is enabled or not
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The name of the login this user is linked to
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// The schema this user will use by default
        /// </summary>
        public string DefaultSchema { get; set; }

        /// <summary>
        /// Any previous names for the user, to indicate whether the user might need to change name
        /// </summary>
        public string[] PreviousNames { get; set; }
    }
}
