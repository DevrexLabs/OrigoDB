using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;

namespace OrigoDB.Core.Storage.Sql
{

    /// <summary>
    /// Command store implementation using a backing Sql database.
    /// </summary>
    public class SqlCommandStore : CommandStore
    {

        readonly SqlSettings _settings;
        readonly DbProviderFactory _providerFactory;
        readonly SqlStatements _statements;

        /// <summary>
        /// Map of NET Data Provider names to Sql statements
        /// </summary>
        public static readonly IDictionary<string, SqlStatements> ProviderStatements
            = new Dictionary<string, SqlStatements>(StringComparer.OrdinalIgnoreCase);

        static SqlCommandStore()
        {
            ProviderStatements["System.Data.SqlClient"] = new MsSqlStatements();
            ProviderStatements["System.Data.SqlServerCe.4.0"] = new MsSqlStatements();
            ProviderStatements["System.Data.OleDb"] = new OleDbStatements();
        }


        public SqlCommandStore(EngineConfiguration config)
            :base(config)
        {
            _settings = config.SqlSettings;
            _settings.ResolveConnectionString();
            _providerFactory = DbProviderFactories.GetFactory(_settings.ProviderName);
            _statements = _settings.Statements ?? ProviderStatements[_settings.ProviderName];
        }

        public override void Initialize()
        {
            base.Initialize();

            //Substitute {0} with table name 
            _statements.AppendEntry = String.Format(_statements.AppendEntry, _settings.TableName);
            _statements.InitStore = String.Format(_statements.InitStore, _settings.TableName);
            _statements.ReadEntries = String.Format(_statements.ReadEntries, _settings.TableName);

            //execute the InitStore statement
            if (!_settings.SkipInit)
            {
                var connection = CreateConnection();
                connection.Open();
                var dbCommand = connection.CreateCommand();
                dbCommand.CommandText = _statements.InitStore;
                using (connection)
                {
                    dbCommand.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Read journal entries by executing ReadEntriesStatement.
        /// Expects 3 columns: Id as long, Created as DateTime and serialized Item as byte[]
        /// </summary>
        /// <returns>a stream of journal entry objects</returns>
        protected override IEnumerable<JournalEntry> GetJournalEntriesFromImpl(ulong entryId)
        {
            var connection = CreateConnection();
            var dbCommand = connection.CreateCommand();
            var parameter = dbCommand.CreateParameter();
            parameter.ParameterName = "@id";
            parameter.DbType = DbType.Int64;
            parameter.Value = entryId;
            dbCommand.Parameters.Add(parameter);
            dbCommand.CommandText = _statements.ReadEntries;
            connection.Open();
            using (connection)
            {
                var reader = dbCommand.ExecuteReader();
                while (reader.Read())
                {
                    var id = (ulong)reader.GetInt64(0);
                    var created = reader.GetDateTime(1);
                    var item = _formatter.FromByteArray<object>((byte[])reader[2]);
                    yield return JournalEntry.Create(id, created, item);
                }
                reader.Close();
            }
        }

        /// <summary>
        /// Not implemented. SqlCommandStore is not stream based.
        /// </summary>
        public override Stream CreateJournalWriterStream(ulong firstEntryId = 1)
        {
            throw new NotImplementedException("SqlCommandStore is not stream based");
        }

        /// <summary>
        /// Creates a SqlJournalWriter
        /// </summary>
        /// <returns></returns>
        protected override IJournalWriter CreateStoreSpecificJournalWriter()
        {
            return new SqlJournalWriter(_formatter, this);
        }

        /// <summary>
        /// Create a parameterized command used to append journal entries.
        /// Caller is responsible for assigning Connection property.
        /// </summary>
        public virtual DbCommand CreateAppendCommand()
        {
            var dbCommand = _providerFactory.CreateCommand();
            dbCommand.CommandText = _statements.AppendEntry;
            
            var param = _providerFactory.CreateParameter();
            param.ParameterName = "@Id";
            param.DbType = DbType.Int64;
            dbCommand.Parameters.Add(param);

            param = _providerFactory.CreateParameter();
            param.ParameterName = "@Created";
            param.DbType = DbType.DateTime;
            dbCommand.Parameters.Add(param);

            param = _providerFactory.CreateParameter();
            param.ParameterName = "@Type";
            param.DbType = DbType.String;
            param.Size = 1024;
            dbCommand.Parameters.Add(param);

            param = _providerFactory.CreateParameter();
            param.ParameterName = "@Data";
            param.DbType = DbType.Binary;
            dbCommand.Parameters.Add(param);


            return dbCommand;
        }

        /// <summary>
        /// Create a closed connection based on this provider and connection string
        /// </summary>
        public virtual DbConnection CreateConnection()
        {
            var connection = _providerFactory.CreateConnection();
            connection.ConnectionString = _settings.ConnectionString;
            return connection;
        }

        /// <summary>
        /// Map the entry to the DbCommands parameters.
        /// </summary>
        internal protected virtual void Bind(JournalEntry entry, DbCommand dbCommand)
        {
            var item = entry.GetItem();
            var typeName = item.GetType().Name;
            var bytes = _formatter.ToByteArray(item);

            dbCommand.Parameters[0].Value = entry.Id;
            dbCommand.Parameters[1].Value = entry.Created;
            dbCommand.Parameters[2].Value = typeName;
            dbCommand.Parameters[3].Value = bytes;
        }


    }
}
