namespace vcdb
{
    public interface IDatabaseDetailsProvider
    {
        /// <summary>
        /// Return the name of the server - e.g. the 'server' part of the connection string
        /// </summary>
        /// <returns></returns>
        string GetServerName();
    }
}