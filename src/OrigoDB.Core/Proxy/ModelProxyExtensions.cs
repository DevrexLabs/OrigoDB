namespace OrigoDB.Core.Proxy
{
    public static class ModelProxyExtensions
    {
        public static TModel GetProxy<TModel>(this IEngine<TModel> engine) where TModel : Model
        {
            return (TModel)new Proxy<TModel>(engine).GetTransparentProxy();
        }
    }
}