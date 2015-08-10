using System;
using System.Data;
using OrigoDB.Core.Utilities;

namespace OrigoDB.Core.Storage.Sql
{
    
    public class SqlJournalWriter : IJournalWriter
    {
        private readonly Func<JournalEntry, IDbCommand> _commandFactory; 

        public SqlJournalWriter(Func<JournalEntry, IDbCommand> commandFactory)
        {
            Ensure.NotNull(commandFactory, "commandFactory");
            _commandFactory = commandFactory;
        }

        public void Write(JournalEntry item)
        {
            var sqlCommand = _commandFactory.Invoke(item);
            sqlCommand.ExecuteNonQuery();
        }

        public void Close()
        {
        }

        public void Dispose()
        {
        }
    }
}
