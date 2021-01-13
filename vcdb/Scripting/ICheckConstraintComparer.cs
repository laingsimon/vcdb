using System.Collections.Generic;
using vcdb.Models;

namespace vcdb.Scripting
{
    public interface ICheckConstraintComparer
    {
        IEnumerable<CheckConstraintDifference> GetDifferentCheckConstraints(
            ComparerContext context,
            IReadOnlyCollection<CheckConstraintDetails> currentChecks,
            IReadOnlyCollection<CheckConstraintDetails> requiredChecks);
    }
}