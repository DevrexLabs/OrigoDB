using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace OrigoDB.Core.Storage.Sql
{
    /// <summary>
    /// Provider from sql based command storage.
    /// </summary>
    public abstract class SqlProvider
    {

        public delegate SqlProvider ProviderConstructor(ConnectionStringSettings settings, String tableName);
     
        public static readonly IDictionary<string, ProviderConstructor> Providers
            = new Dictionary<string, ProviderConstructor>();

        /// <summary>
        /// Register a custom SqlProvider implementation to be able to 
        /// </summary>
        /// <param name="providerName"></param>
        /// <param name="constructor"></param>
        public static void Register(string providerName, ProviderConstructor constructor)
        {
            Providers[providerName] = constructor;
        }

        static SqlProvider()
        {
            Register(MsSqlProvider.ProviderName, (settings,table) => new MsSqlProvider(settings,table));
        }

        protected readonly DbProviderFactory ProviderFactory;
        protected readonly string TableName;
        protected readonly string ConnectionString;


        /// <summary>
        /// A sql statement that returns entryid, created and item as byte[]
        /// </summary>
        public string ReadEntriesStatement { get; set; }

        /// <summary>
        /// A sql statement that ensures the required database objects exist
        /// </summary>
        public string InitStatement { get; set; }

        /// <summary>
        /// A sql statement with 4 params, entryId, created, item type and item as byte[]
        /// </summary>
        public string AppendEntryStatement { get; set; }

        protected SqlProvider(ConnectionStringSettings settings, string tableName)
        {
            TableName = tableName;
            ProviderFactory = DbProviderFactories.GetFactory(settings.ProviderName);
            ConnectionString = settings.ConnectionString;
        }

        /// <summary>
        /// Read journal entries by executing ReadEntriesStatement.
        /// Expects 3 columns: Id as long, Created as DateTime and serialized Item as byte[]
        /// </summary>
        /// <param name="firstEntry">id of first entry to</param>
        /// <param name="deserializer"></param>
        /// <returns></returns>
        public virtual IEnumerable<JournalEntry> ReadJournalEntries(ulong firstEntry, Func<byte[], object> deserializer)
        {
            var connection = CreateConnection();
            var dbCommand = connection.CreateCommand();
            var parameter = dbCommand.CreateParameter();
            parameter.ParameterName = "@id";
            parameter.DbType = DbType.Int64;
            parameter.Value = firstEntry;
            dbCommand.Parameters.Add(parameter);
            dbCommand.CommandText = ReadEntriesStatement;
            connection.Open();
            using (connection)
            {
                var reader = dbCommand.ExecuteReader();
                while (reader.Read())
                {
                    var id = (ulong) reader.GetInt64(0);
                    var created = reader.GetDateTime(1);
                    var item = deserializer.Invoke((byte[]) reader[2]);
                    yield return JournalEntry.Create(id, created, item);
                }
                reader.Close();
            }
        }

        /// <summary>
        /// Ensure db objects exist and are valid by executing the InitStatement
        /// </summary>
        public virtual void Initialize()
        {
            var connection = CreateConnection();
            connection.Open();
            var dbCommand = connection.CreateCommand();
            dbCommand.CommandText = InitStatement;
            using (connection)
            {
                dbCommand.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Create a parameterized command used to append journal entries.
        /// Caller is responsible for assigning Connection property.
        /// </summary>
        public virtual DbCommand CreateAppendCommand()
        {
            var dbCommand = ProviderFactory.CreateCommand();
            dbCommand.CommandText = AppendEntryStatement;
            
            var param = ProviderFactory.CreateParameter();
            param.ParameterName = "@Id";
            param.DbType = DbType.Int64;
            dbCommand.Parameters.Add(param);

            param = ProviderFactory.CreateParameter();
            param.ParameterName = "@Created";
            param.DbType = DbType.DateTime;
            dbCommand.Parameters.Add(param);

            param = ProviderFactory.CreateParameter();
            param.ParameterName = "@Type";
            param.DbType = DbType.String;
            param.Size = 1024;
            dbCommand.Parameters.Add(param);

            param = ProviderFactory.CreateParameter();
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
            var connection = ProviderFactory.CreateConnection();
            connection.ConnectionString = ConnectionString;
            return connection;
        }

        /// <summary>
        /// Map the entry to the DbCommands parameters.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="dbCommand">A command previously aquired from call to CreateAppendCommand()</param>
        /// <param name="id"></param>
        /// <param name="created"></param>
        /// <param name="type"></param>
        internal virtual void Bind(ulong id, DateTime created, string type, byte[] payload, DbCommand dbCommand)
        {
            dbCommand.Parameters[0].Value = id;
            dbCommand.Parameters[1].Value = created;
            dbCommand.Parameters[2].Value = type;
            dbCommand.Parameters[3].Value = payload;
        }

        internal static SqlProvider Create(ConnectionStringSettings settings, string tableName)
        {
            var providerName = settings.ProviderName;
            if (!Providers.ContainsKey(providerName))
            {
                throw new ArgumentException("No such provider:" + providerName);
            }
            return Providers[providerName].Invoke(settings, tableName);
        }
    }
}