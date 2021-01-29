using System;
using System.Threading.Tasks;
using vcdb.CommandLine;

namespace vcdb.Output
{
    internal class ScriptOutputHeader : IScriptOutputHeader
    {
        private readonly Options options;
        private readonly DatabaseVersion version;
        private readonly IDatabaseDetailsProvider databaseDetailsProvider;
        private readonly IInput input;

        internal const string FixedHeader = @"This script has been created by vcdb using the settings below. Review the script before running it.
The usage of the script is at your discretion, the author/s of vcdb accept no responsibility or liability should the script cause unintended side-effects.
Project site: https://github.com/laingsimon/vcdb";

        public ScriptOutputHeader(
            Options options,
            DatabaseVersion version,
            IDatabaseDetailsProvider databaseDetailsProvider,
            IInput input)
        {
            this.options = options;
            this.version = version;
            this.databaseDetailsProvider = databaseDetailsProvider;
            this.input = input;
        }

        public async Task WriteToOutput(IOutput output)
        {
            await output.WriteToOutput(GetHeader());
        }

        private string GetHeader()
        {
            return $@"/*
{FixedHeader}
{GetHeaderDetail()}*/

";
        }

        private string GetHeaderDetail()
        {
            if (options.ExcludeDetailInScriptHeader)
            {
                return null;
            }

            return $@"
vcdb version     : {GetAssemblyVersion(this)}
Created at (utc) : {DateTime.UtcNow:F}
Created by       : {Environment.UserName ?? "unknown"}
Created using    : {options.InputFile} (MD5: {input.GetHash(16)}) ({Environment.OSVersion})
Compared against : {databaseDetailsProvider.GetServerName()}, Database: {options.Database}
Script builder   : {databaseDetailsProvider.GetType().Assembly.GetName().Name}, {GetAssemblyVersion(databaseDetailsProvider)}
Compatible with  : {version.ProductName} {version.MinimumCompatibilityVersion}
";
        }

        private Version GetAssemblyVersion(object instance)
        {
            return instance.GetType().Assembly.GetName().Version;
        }
    }
}
