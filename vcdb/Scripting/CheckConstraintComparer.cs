using System.Collections.Generic;
using System.Linq;
using vcdb.Models;

namespace vcdb.Scripting
{
    public class CheckConstraintComparer : ICheckConstraintComparer
    {
        public IEnumerable<CheckConstraintDifference> GetDifferentCheckConstraints(
            IReadOnlyCollection<CheckConstraintDetails> currentChecks,
            IReadOnlyCollection<CheckConstraintDetails> requiredChecks)
        {
            var processedChecks = new HashSet<CheckConstraintDetails>();
            foreach (var requiredCheck in requiredChecks)
            {
                var currentCheck = GetCurrentItem(currentChecks, requiredCheck);

                if (currentCheck == null)
                {
                    yield return new CheckConstraintDifference
                    {
                        RequiredConstraint = requiredCheck,
                        ConstraintAdded = true
                    };
                }
                else
                {
                    processedChecks.Add(currentCheck);

                    var difference = new CheckConstraintDifference
                    {
                        CurrentConstraint = currentCheck,
                        RequiredConstraint = requiredCheck,
                        CheckRenamedTo = currentCheck.Name != requiredCheck.Name
                            ? requiredCheck.Name.AsChange()
                            : null,
                        CheckChangedTo = currentCheck.Check != requiredCheck.Check
                            ? requiredCheck.Check
                            : null
                    };

                    if (difference.IsChanged)
                        yield return difference;
                }
            }

            foreach (var currentCheck in currentChecks.Where(col => !processedChecks.Contains(col)))
            {
                yield return new CheckConstraintDifference
                {
                    CurrentConstraint = currentCheck,
                    ConstraintDeleted = true
                };
            }
        }

        private CheckConstraintDetails GetCurrentItem(
            IReadOnlyCollection<CheckConstraintDetails> currentChecks,
            CheckConstraintDetails requiredCheck)
        {
            return GetSameNamedItem(currentChecks, requiredCheck.Name)
                ?? GetRenamedItem(currentChecks, requiredCheck.PreviousNames)
                ?? GetSameValuedItem(currentChecks, requiredCheck.Check);
        }

        private CheckConstraintDetails GetSameValuedItem(
            IReadOnlyCollection<CheckConstraintDetails> currentChecks,
            string check)
        {
            return currentChecks.SingleOrDefault(chk => chk.Check == check);
        }

        private CheckConstraintDetails GetRenamedItem(
            IReadOnlyCollection<CheckConstraintDetails> currentChecks,
            string[] previousNames)
        {
            if (previousNames == null)
                return null;

            return previousNames
                .Select(name => GetSameNamedItem(currentChecks, name))
                .FirstOrDefault(check => check != null);
        }

        private CheckConstraintDetails GetSameNamedItem(
            IReadOnlyCollection<CheckConstraintDetails> currentChecks, 
            string name)
        {
            return currentChecks.SingleOrDefault(check => check.Name == name);
        }
    }
}
