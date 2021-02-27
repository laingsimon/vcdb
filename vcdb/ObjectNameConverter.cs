using System.Linq;
using System.Text.RegularExpressions;

namespace vcdb
{
    public class ObjectNameConverter : IObjectNameConverter
    {
        private readonly string delimiter;
        private readonly string startName;
        private readonly string endName;

        public ObjectNameConverter(
            string delimiter = ".",
            string startName = null,
            string endName = null)
        {
            this.delimiter = delimiter ?? "";
            this.startName = startName ?? "";
            this.endName = endName ?? "";
        }

        /// <summary>
        /// Format the given names for the given database product.
        /// 
        /// For SQL Server this might encapsure each name within square brackets.
        /// </summary>
        /// <param name="names"></param>
        /// <returns></returns>
        public string ConvertToString(params string[] names)
        {
            return string.Join(delimiter, names.Where(n => !string.IsNullOrEmpty(n)).Select(name => $"{startName}{name}{endName}"));
        }

        /// <summary>
        /// Return a match that can extract a schema-name and table-name from the given input
        /// The returned match should have 2 named groupes, one called 'schema' and the other called 'table'
        /// 
        /// For SQL Server this would extract the names from a format such as '[schema].[table]'
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Match ExtractFromString(string input)
        {
            return Regex.Match(input, $@"^{Regex.Escape(startName)}?(?<schema>.+?){Regex.Escape(endName)}?{Regex.Escape(delimiter)}{Regex.Escape(startName)}?(?<table>.+?){Regex.Escape(endName)}?$");
        }
    }
}
