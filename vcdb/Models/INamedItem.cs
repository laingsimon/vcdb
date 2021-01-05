namespace vcdb.Models
{
    public interface INamedItem<TKey>
    {
        TKey[] PreviousNames { get; }
    }
}
