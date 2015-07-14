namespace OrigoDB.Test.NUnit.GenericDomain
{
    public interface IDomainElement<TKey>
    {
        TKey Id { get; set; }
    }
}