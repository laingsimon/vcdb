using System.Diagnostics;

namespace vcdb.Scripting
{
    /// <summary>
    /// Whether a value has changed, possibly *to* null
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerDisplay("Change to {Value,nq}")]
    public class Change<T>
    {
        public Change(T changedTo)
        {
            Value = changedTo;
        }

        public T Value { get; }
    }
}
