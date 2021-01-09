using System.Collections.Generic;
using vcdb.Models;

namespace vcdb.Scripting
{
    public interface ICheckConstraintComparer
    {
        IEnumerable<CheckConstraintDifference> GetDifferentCheckConstraints(IReadOnlyCollection<CheckConstraintDetails> currentChecks, IReadOnlyCollection<CheckConstraintDetails> requiredChecks);
    }
}