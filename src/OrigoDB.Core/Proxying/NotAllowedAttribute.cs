namespace Proxying
{
    internal class NotAllowedAttribute : OperationAttribute
    {
        public static readonly OperationAttribute Default = new NotAllowedAttribute();

    }
}