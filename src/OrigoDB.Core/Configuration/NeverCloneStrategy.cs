namespace OrigoDB.Core
{
    public sealed class NeverCloneStrategy : CloneStrategy {
        internal override void Apply(ref Command command)
        {
            
        }

        internal override void Apply(ref object result, object producer)
        {
        }
    }
}