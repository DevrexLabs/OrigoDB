using Todo.Core;
using LiveDomain.Core;
using LiveDomain.Enterprise;

namespace Todo.Wpf
{
    public class ConnectionSettingsViewModel
    {
        public string Host { get; set; }
        public ushort Port { get; set; }
        public bool? IsEmbedded { get; set; }

        public bool IsRemote { get; set; }

        public ConnectionSettingsViewModel()
        {
            Host = "localhost";
            Port = 9292;
            IsEmbedded = false;
            IsRemote = true;
        }

        public ITransactionHandler<TodoModel> GetTransactionHandler()
        {
            if(IsEmbedded.Value) return Engine.LoadOrCreate<TodoModel>();
            
            var client = new LiveDomainClient<TodoModel>(Host, Port);
            client.Open();
            return client;
        }
    }
}
