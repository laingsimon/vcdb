using DiffPlex.Chunkers;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestFramework
{
    internal class ScriptDiffer : IScriptDiffer
    {
        private readonly IInlineDiffBuilder differ;
        private readonly bool ignoreCase = false;
        private readonly bool ignoreWhitespace = true;

        public ScriptDiffer(IInlineDiffBuilder differ)
        {
            this.differ = differ;
        }

        public IEnumerable<string> CompareScripts(TextReader expected, string actual)
        {
            var expectedOutput = expected.ReadToEnd().Trim();
            var diffs = differ.BuildDiffModel(
                expectedOutput,
                actual?.Trim() ?? "",
                ignoreWhitespace,
                ignoreCase,
                new LineChunker());

            return FormatDiff(diffs.Lines);
        }

        private IEnumerable<string> FormatDiff(IEnumerable<DiffPiece> lines)
        {
            var diffBlock = new List<DiffPiece>();
            DiffPiece lastDiff = null;

            foreach (var line in lines)
            {
                var captureDiff = line.Type == ChangeType.Deleted || line.Type == ChangeType.Inserted;
                if (captureDiff && lastDiff != null)
                {
                    diffBlock.Add(lastDiff);
                    lastDiff = null;
                }

                if (captureDiff)
                {
                    diffBlock.Add(line);
                    continue;
                }
                else if (diffBlock.Any())
                {
                    //add the line as the last line of context
                    diffBlock.Add(line);
                    foreach (var diffDetail in FormatDiffBlock(diffBlock))
                        yield return diffDetail;
                    diffBlock = new List<DiffPiece>();
                }

                lastDiff = line;
            }

            if (diffBlock.Any())
            {
                foreach (var diffDetail in FormatDiffBlock(diffBlock))
                    yield return diffDetail;
            }
        }

        private IEnumerable<string> FormatDiffBlock(List<DiffPiece> diffBlock)
        {
            var startingLine = diffBlock[0].Position;
            var endingLine = diffBlock.LastOrDefault(l => l.Position != null)?.Position;

            yield return $@"Lines {startingLine}..{endingLine} (vcdb output vs ExpectedOutput.json)";

            foreach (var line in diffBlock)
            {
                switch (line.Type)
                {
                    case ChangeType.Inserted:
                        yield return $"+ {line.Text}";
                        break;
                    case ChangeType.Deleted:
                        yield return $"- {line.Text}";
                        break;
                    default:
                        yield return $"  {line.Text}";
                        break;
                }
            }
        }
    }
}
