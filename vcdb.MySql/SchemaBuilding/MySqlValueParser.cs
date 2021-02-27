using System;
using System.Text.RegularExpressions;

namespace vcdb.MySql.SchemaBuilding
{
    public class MySqlValueParser : IMySqlValueParser
    {
        public object ParseDefault(string stringDefinition)
        {
            if (string.IsNullOrEmpty(stringDefinition))
                return stringDefinition;

            if (stringDefinition.StartsWith("\"") && stringDefinition.EndsWith("\""))
                return stringDefinition.Trim('\"');

            if (int.TryParse(stringDefinition, out var intValue))
                return intValue;

            if (decimal.TryParse(stringDefinition, out var decimalValue))
                return decimalValue;

            return ParseValue(stringDefinition);
        }

        public object ParseValue(string definition)
        {
            var byteMatch = Regex.Match(definition, @"^([Bb]'(?<value>\d+)')|(0b(?<value>\d+))$");
            if (byteMatch.Success)
            {
                return byte.Parse(byteMatch.Groups["value"].Value);
            }

            var hexadecimalMatch = Regex.Match(definition, @"^([Xx]'(?<value>.+?)')|(0x(?<value>[0-9A-Fa-f]+?))$");
            if (hexadecimalMatch.Success)
            {
                return Convert.ToByte(hexadecimalMatch.Groups["value"].Value, 16);
            }

            return definition;
        }
    }
}
