using System;
using OrigoDB.Core;

namespace OrigoDB.Models.Redis
{
    [Serializable]
    public class PurgeExpiredKeysCommand : Command<RedisModel>
    {
        public override void Execute(RedisModel model)
        {
                model.PurgeExpired();
        }
    }
}
