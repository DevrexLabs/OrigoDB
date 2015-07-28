using System;

namespace OrigoDB.Core.Modeling.Redis
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
