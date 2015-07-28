using System;

namespace OrigoDB.Core.Modeling.Redis
{
    public static class Extensions
    {
        public static bool Expire(this RedisModel redis, string key, TimeSpan after)
        {
            var expires = DateTime.Now + after;
            return redis.Expire(key, expires);
        }
    }
}