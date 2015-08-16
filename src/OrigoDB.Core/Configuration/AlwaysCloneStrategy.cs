namespace OrigoDB.Core
{
    public sealed class AlwaysCloneStrategy : CloneStrategy {
        internal override void Apply(ref Command command)
        {
            command = Formatter.Clone(command);
        }

        internal override void Apply(ref object result, object producer)
        {
            result = Formatter.Clone(result);
        }
    }
}