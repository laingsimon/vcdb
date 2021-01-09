namespace vcdb
{
    public interface IHashHelper
    {
        string GetHash(string input, int? hashSize = null);
    }
}