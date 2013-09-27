namespace OrigoDB.Core.Proxy
{
    public static class ModelProxyExtensions
    {
        public static M GetProxy<M>(this IEngine<M> engine) where M : Model
        {
            return (M)new ModelProxy<M>(engine).GetTransparentProxy();
        }
    }
}