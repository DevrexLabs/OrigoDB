using System.Data.Common;
using System.Runtime.Serialization;
using OrigoDB.Core.Utilities;

namespace OrigoDB.Core.Storage.Sql
{  
    internal class SqlJournalWriter : IJournalWriter
    {
        private readonly SqlCommandStore _commandStore;
        private readonly DbConnection _connection;
        private readonly DbCommand _preparedCommand;
        private readonly IFormatter _formatter;

        public SqlJournalWriter(IFormatter formatter, SqlCommandStore commandStore)
        {
            Ensure.NotNull(commandStore, "commandStore");
            Ensure.NotNull(formatter, "formatter");
            _formatter = formatter;
            _commandStore = commandStore;
            _connection = commandStore.CreateConnection();
            _connection.Open();
            _preparedCommand = commandStore.CreateAppendCommand();
            _preparedCommand.Connection = _connection;
        }

        public void Write(JournalEntry entry)
        {
            _commandStore.Bind(entry, _preparedCommand);
            _preparedCommand.ExecuteNonQuery();
        }

        public void Close()
        {
        }

        public void Dispose()
        {
            _connection.Close();
        }


        public void Handle(SnapshotCreated snapshotCreated)
        {
            //no op
        }
    }
}
