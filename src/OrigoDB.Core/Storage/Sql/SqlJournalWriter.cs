using System.Data.Common;
using System.Runtime.Serialization;
using OrigoDB.Core.Utilities;

namespace OrigoDB.Core.Storage.Sql
{  
    internal class SqlJournalWriter : IJournalWriter
    {
        private readonly SqlProvider _provider;
        private readonly DbConnection _connection;
        private readonly DbCommand _preparedCommand;
        private readonly IFormatter _formatter;

        public SqlJournalWriter(IFormatter formatter, SqlProvider provider)
        {
            Ensure.NotNull(provider, "provider");
            Ensure.NotNull(formatter, "formatter");
            _provider = provider;
            _formatter = formatter;
         
            _connection = provider.CreateConnection();
            _connection.Open();
            _preparedCommand = provider.CreateAppendCommand();
            _preparedCommand.Connection = _connection;
        }

        public void Write(JournalEntry entry)
        {
            var typeName = entry.GetItem().GetType().Name;
            var item = entry.GetItem();
            var bytes = _formatter.ToByteArray(item);
            _provider.Bind(entry.Id, entry.Created, typeName, bytes, _preparedCommand);
            _preparedCommand.ExecuteNonQuery();
        }

        public void Close()
        {
        }

        public void Dispose()
        {
            _connection.Close();
        }
    }
}
