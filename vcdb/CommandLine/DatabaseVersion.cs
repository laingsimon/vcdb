using System;
using System.Text.RegularExpressions;

namespace vcdb.CommandLine
{
    public class DatabaseVersion
    {
        public const string Default = "SqlServer^2014";

        /// <summary>
        /// The name of the database product, such as SqlServer, MySql, etc.
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// The version any sql script should be created to be compatible with
        /// This can also inform the 'reading' process on the type of statements that may need to be used to read the current schema
        /// </summary>
        public string MinimumCompatibilityVersion { get; set; }

        public static DatabaseVersion Parse(string input)
        {
            var match = Regex.Match(input ?? "", @"^((?<product>.+?)\^(?<version>.+))|((?<product>.+?)\^?)$");
            if (string.IsNullOrWhiteSpace(input) 
                || !match.Success 
                || string.IsNullOrEmpty(match.Groups["product"].Value) 
                || match.Groups["product"].Value.Contains("^") 
                || match.Groups["version"].Value.Contains("^"))
            {
                throw new FormatException($"Input `{input}` is not in a valid database product name & version format, use `<productname>` or `<productname>^<minimum compatibility version>` (without the back ticks)");
            }

            var productName = match.Groups["product"].Value;
            var version = match.Groups["version"].Value;
            if (string.IsNullOrWhiteSpace(version))
            {
                return new DatabaseVersion { ProductName = productName };
            }

            return new DatabaseVersion 
            { 
                ProductName = productName,
                MinimumCompatibilityVersion = version
            };
        }
    }
}
