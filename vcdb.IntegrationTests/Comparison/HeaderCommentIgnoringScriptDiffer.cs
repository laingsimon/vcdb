using System.Collections.Generic;

namespace vcdb.IntegrationTests.Comparison
{
    internal class HeaderCommentIgnoringScriptDiffer : IScriptDiffer
    {
        private readonly ScriptDiffer underlyingDiffer;

        public HeaderCommentIgnoringScriptDiffer(ScriptDiffer underlyingDiffer)
        {
            this.underlyingDiffer = underlyingDiffer;
        }

        public IEnumerable<Difference> CompareScripts(string expected, string actual)
        {
            //skip the comment section
            var endOfCommentIndex = actual.IndexOf("*/");
            if (endOfCommentIndex != -1)
            {
                actual = actual.Substring(endOfCommentIndex + 2).TrimStart();
            }

            return underlyingDiffer.CompareScripts(expected, actual);
        }
    }
}
