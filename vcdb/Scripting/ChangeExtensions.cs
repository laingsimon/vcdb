namespace vcdb.Scripting
{
    public static class ChangeExtensions
    {
        public static Change<T> AsChange<T>(this T value)
        {
            return new Change<T>(value);
        }
    }
}
