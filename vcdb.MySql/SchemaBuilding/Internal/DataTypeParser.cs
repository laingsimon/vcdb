using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace vcdb.MySql.SchemaBuilding.Internal
{
    public class DataTypeParser : IDataTypeParser
    {
        private static readonly Dictionary<string, int> Widths = new Dictionary<string, int>
        {
            { "bit", 1 },
            { "int", 11 }
        };

        /// <summary>
        /// From this page: https://bugs.mysql.com/bug.php?id=69620
        /// 
        /// "nvarchar in MySQL is just an alias for VARCHAR and CHARACTER SET utf8. If you set the default charset in your DB and/or tables to utf8 you should be fine with VARCHAR in your columns"
        /// 
        /// This is the name of the utf8 collation, if it is set, then consider any varchar() or char() columns to be nvarchar() or nchar() instead
        /// </summary>
        private const string Utf8Collation = "utf8_general_ci";

        public string GetDataType(string type, string collation)
        {
            if (IsNationalCharacterColumn(type, collation))
            {
                return "n" + type;
            }

            var typeWithoutWidth = Regex.Match(type, @"(?<typeName>.+)\((?<width>\d+)\)");
            if (!typeWithoutWidth.Success)
            {
                return type;
            }

            var typeName = typeWithoutWidth.Groups["typeName"].Value;
            var width = int.Parse(typeWithoutWidth.Groups["width"].Value);

            var defaultWidth = Widths.ItemOrDefault(typeName);
            if (width == defaultWidth)
            {
                return typeName;
            }

            return type;
        }

        public bool IsNationalCharacterColumn(string type, string collation)
        {
            var isUtf8Collation = collation == Utf8Collation;
            return (type.StartsWith("varchar(") || type.StartsWith("char(")) && isUtf8Collation;
        }
    }
}
