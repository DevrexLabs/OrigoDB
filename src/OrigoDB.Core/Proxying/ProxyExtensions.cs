using Proxying;

namespace OrigoDB.Core
{
    public static class ProxyExtensions
    {
        public static T GetProxy<T>(this IEngine<T> engine) where T : Model
        {
            return (T)new Proxy<T>(engine).GetTransparentProxy();
        }
    }
}