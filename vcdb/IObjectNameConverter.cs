using System.Text.RegularExpressions;

namespace vcdb
{
    public interface IObjectNameConverter
    {
        string ConvertToString(params string[] names);
        Match ExtractFromString(string input);
    }
}