using DiffPlex.Chunkers;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using System.Collections.Generic;
using System.Linq;

namespace vcdb.IntegrationTests.Comparison
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

        public IEnumerable<Difference> CompareScripts(string expected, string actual)
        {
            var expectedOutput = expected.Trim();
            var diffs = differ.BuildDiffModel(
                expectedOutput,
                actual?.Trim() ?? "",
                ignoreWhitespace,
                ignoreCase,
                new LineChunker());

            return CreateDifferences(diffs.Lines);
        }

        private IEnumerable<Difference> CreateDifferences(IEnumerable<DiffPiece> lines)
        {
            var unchanged = new List<Line>();
            var deleted = new List<Line>();
            var added = new List<Line>();

            foreach (var lineDiff in lines)
            {
                var line = new Line(lineDiff.Position, lineDiff.Text);

                switch (lineDiff.Type)
                {
                    case ChangeType.Deleted:
                        deleted.Add(line);
                        break;
                    case ChangeType.Inserted:
                        added.Add(line);
                        break;
                    default:
                        if (deleted.Any() || added.Any())
                        {
                            yield return new Difference
                            {
                                Before = unchanged.ToArray(),
                                Actual = added.ToArray(),
                                Expected = deleted.ToArray()
                            };
                            unchanged.Clear();
                            added.Clear();
                            deleted.Clear();
                        }

                        unchanged.Add(line);
                        break;
                }
            }

            if (deleted.Any() || added.Any())
            {
                yield return new Difference
                {
                    Before = unchanged.ToArray(),
                    Actual = added.ToArray(),
                    Expected = deleted.ToArray()
                };
            }
        }
    }
}
