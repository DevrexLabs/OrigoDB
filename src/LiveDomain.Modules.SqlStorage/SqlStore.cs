using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using LiveDomain.Core;
using System.Data;
using System.IO;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Data.Odbc;
using LiveDomain.Core.Storage;

namespace LiveDomain.Modules.SqlStorage
{
    public class SqlStore  : Store
    {
        private DbProviderFactory _dbProviderFactory;
        private string _connectionString;
        private FileStore _fileStore;
        private string _tableName = SqlEngineConfiguration.DefaultJournalTableName;


        public SqlStore(SqlEngineConfiguration config) : base(config)
        {
            _tableName = config.JournalTableName;
            if(config.LocationType == LocationType.ConnectionString)
            {
                ConfigureProviderFactory(config.RelativeLocation, config.ProviderName);
            }
            else
            {
                ConfigureProviderFromConnectionStringName();                
            }
            Configure();
        }

        private void ConfigureProviderFromConnectionStringName()
        {
            string connectionStringName = _config.RelativeLocation;
            var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName];
            ConfigureProviderFactory(connectionString.ConnectionString, connectionString.ProviderName);
            
        }

        private void ConfigureProviderFactory(string connectionString, string providerName)
        {
            _connectionString = connectionString;
            _dbProviderFactory = DbProviderFactories.GetFactory(providerName);
        }
        
        private void Configure()
        {
            var fileStoreConfig = new EngineConfiguration(_config.SnapshotLocation);
            _fileStore = new FileStore(fileStoreConfig);
        }

        public SqlStore(EngineConfiguration config) : base(config)
        {
            ConfigureProviderFromConnectionStringName();
            Configure();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<JournalEntry<Command>> GetJournalEntriesFrom(long lastEntryId)
        {
            ISerializer serializer = _config.CreateSerializer();
            string sql = GetEntrySelectStatement(lastEntryId);
            var dbCommand = CreateSqlCommand(sql);
            var connection = dbCommand.Connection;
            
            using (connection)
            {
                var reader = dbCommand.ExecuteReader();
                while (reader.Read())
                {
                    long entryId = reader.GetInt64(0);
                    long length = reader.GetInt64(1);
                    if(length > Int32.MaxValue) throw new OverflowException("serialized journal entry is too big");
                    byte[] buffer = new byte[length];
                    reader.GetBytes(2, 0, buffer, 0, (int)length);
                    var entry = serializer.Deserialize<JournalEntry<Command>>(buffer);
                    if(entry.Id != entryId) throw new Exception("EntryId mismatch in database, id =" + entryId);
                    yield return entry;
                }
            }
        }


        private string GetEntrySelectStatement(long startingEntryId)
        {
            string sql = null;
            if (_dbProviderFactory is SqlClientFactory) sql = "SELECT id, len(Entry), Entry FROM [{0}] WHERE Id >= {1} order by Id";
            else if (_dbProviderFactory is OleDbFactory) sql = "SELECT id, len(Entry), Entry FROM [{0}] WHERE Id >= {1} order by Id";
            else if (_dbProviderFactory is OdbcFactory) sql = "SELECT id, len(Entry), Entry FROM [{0}] WHERE Id >= {1} order by Id";
            else throw new NotSupportedException("The database provider is not supported");
            return String.Format(sql, _tableName, startingEntryId);
        }

        private IDbConnection GetConnection()
        {
            var connection = _dbProviderFactory.CreateConnection();
            connection.ConnectionString = _connectionString;
            return connection;
        }

        /// <summary>
        /// Can connect to database, tables exist
        /// </summary>
        public override void VerifyCanLoad()
        {
            try
            {
                string sql = string.Format("SELECT Count(*) From [{0}]", _tableName);
                var command = CreateSqlCommand(sql);
                using (command.Connection)
                {
                    command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("cant restore database from storage", ex);
            }
        }
        
        public override Model LoadMostRecentSnapshot(out long lastEntryId)
        {
            return _fileStore.LoadMostRecentSnapshot(out lastEntryId);
        }


        private Model LoadMostRecentSnapshotFromDb(out long lastEntryId)
        {
            lastEntryId = -1;
            string sql = "select id from Snapshots order by id desc";
            var cmd = CreateSqlCommand(sql);
            using (cmd.Connection)
            {
                cmd.Connection.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    long id = reader.GetInt64(0);
                    var snapshot = new FileSnapshot(DateTime.MinValue, id);
                    string path = Path.Combine(_config.SnapshotLocation, snapshot.Name);
                    if (File.Exists(path))
                    {
                        lastEntryId = id;
                        using (var stream = File.OpenRead(path))
                        {
                            return _config.CreateSerializer().Read<Model>(stream);
                        }
                    }
                }
            }
            return null;
        }

        protected override Snapshot WriteSnapshotImpl(Model model, long lastEntryId)
        {
            _fileStore.WriteSnapshot(model, lastEntryId);
            return _fileStore.Snapshots.Last();

            //var snapshot = new FileSnapshot(DateTime.Now, lastEntryId);
            //string path = Path.Combine(_config.SnapshotLocation, snapshot.Name);
            //var stream = File.OpenWrite(path);
            //using (stream)
            //{
            //    _config.CreateSerializer().Write(model, stream);
            //}

            ////add a row in the database
            //string sql = String.Format("INSERT Snapshots VALUES({0}, getdate(), '{1}')", lastEntryId, path);
            //var connection = GetConnection();
            //using (connection)
            //{
            //    connection.Open();
            //    var cmd = connection.CreateCommand();
            //    cmd.CommandText = sql;
            //    cmd.ExecuteNonQuery();
            //}
            //return snapshot;
        }

        /// <summary>
        /// Database exists, but tables dont
        /// </summary>
        public override void VerifyCanCreate()
        {
            string sql = string.Format("select count(*) from information_schema.tables where table_name = '{0}'", _tableName);
            var cmd = CreateSqlCommand(sql);
            using (cmd.Connection)
            {
                int rows = (int) cmd.ExecuteScalar();
                if (rows > 0) throw new Exception("Cant create storage, already exists");
            }
        }

        protected override IJournalWriter CreateStoreSpecificJournalWriter(long lastEntryId)
        {
           return new SqlJournalWriter(this);
        }

        public override IEnumerable<JournalEntry<Command>> GetJournalEntriesBeforeOrAt(DateTime pointInTime)
        {
            foreach (JournalEntry<Command> journalEntry in GetJournalEntriesFrom(1))
            {
                if (journalEntry.Created <= pointInTime) yield return journalEntry;
            }
        }

        public override void Create(Model model)
        {
            string sql = GetCreateStatement();
            var command = CreateSqlCommand(sql);
            using(command.Connection) command.ExecuteNonQuery();

            _fileStore.Create(model);
            _fileStore.Load();
            Load();
        }

        protected override IEnumerable<Snapshot> LoadSnapshots()
        {
            _fileStore.Load();
            return _fileStore.Snapshots;
        }

        private IDbCommand CreateSqlCommand(string sql)
        {
            var connection = GetConnection();
            var command = connection.CreateCommand();
            command.CommandText = sql;
            connection.Open();
            return command;
        }

        internal void WriteEntry(JournalEntry item)
        {
            string commandName = ((JournalEntry<Command>) item).Item.GetType().Name;
            string sqlTemplate = "insert [{0}] values({1}, '{2}', @created, @entry)";
            string sql = String.Format(sqlTemplate, _tableName, item.Id, commandName);
            var command = CreateSqlCommand(sql);

            var createdParam = command.CreateParameter();
            createdParam.ParameterName = "@created";
            createdParam.Value = item.Created;
            command.Parameters.Add(createdParam);

            var entryParam = command.CreateParameter();
            entryParam.ParameterName = "@entry";
            entryParam.Value = _serializer.Serialize(item);
            command.Parameters.Add(entryParam);

            using (command.Connection) command.ExecuteNonQuery();
        }


        private string GetCreateStatement()
        {
            return String.Format(CreateTableCommand, _tableName);
        }

        private const string CreateTableCommand = @"create table [{0}](
	Id bigint not null primary key,
	CommandName varchar(500) not null,
	Created datetime not null,
	Entry varbinary(max) not null)";


    }
}
