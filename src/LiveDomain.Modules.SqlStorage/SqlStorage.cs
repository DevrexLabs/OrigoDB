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

namespace LiveDomain.Modules.SqlStorage
{
    public class SqlStorage // : IStorage
    {
        private EngineConfiguration _config;
        private DbProviderFactory _dbProviderFactory;
        private string _connectionString;

        public SqlStorage(EngineConfiguration config)
        {
            _config = config;

            string connectionStringName = "livedbstorage";//_config.Location;
            var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName];
            string providerName = connectionString.ProviderName;
            _connectionString = connectionString.ConnectionString;
            //_connectionString = "Data Source=.;Initial Catalog=livedb;Integrated Security=True";
            _dbProviderFactory = DbProviderFactories.GetFactory(providerName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<JournalEntry<Command>> GetJournalEntries(long lastEntryId)
        {
            ISerializer serializer = _config.CreateSerializer();
            string sql = GetSelectStatement(lastEntryId);
            IDbConnection conn = GetConnection();
            using (conn)
            {
                conn.Open();
                IDbCommand dbCommand = conn.CreateCommand(); 
                dbCommand.CommandText = sql;
                var reader = dbCommand.ExecuteReader();
                while (reader.Read())
                {
                    int length = reader.GetInt32(0);
                    byte[] buffer = new byte[length];
                    reader.GetBytes(1, 0, buffer, 0, length);
                    var entry = serializer.Deserialize<JournalEntry<Command>>(buffer);
                    yield return entry;
                }
            }
        }

        private string GetSelectStatement(long sequenceNumber)
        {
            string sql = null;
            if (_dbProviderFactory is SqlClientFactory) sql = "SELECT len(JournalEntry), JournalEntry FROM CommandJournal WHERE Id > {0} order by Id";
            else if (_dbProviderFactory is OleDbFactory) sql = "SELECT len(JournalEntry), JournalEntry FROM CommandJournal WHERE Id > {0} order by Id";
            else if (_dbProviderFactory is OdbcFactory) sql = "SELECT len(JournalEntry), JournalEntry FROM CommandJournal WHERE Id > {0} order by Id";
            else throw new NotSupportedException("The database provider is not supported");
            return String.Format(sql, sequenceNumber);
        }

        private IDbConnection GetConnection()
        {
            var connection = _dbProviderFactory.CreateConnection();
            connection.ConnectionString = _connectionString;
            return connection;
        }

        /// <summary>
        /// Can connect to database, tables exist and contain data (?)
        /// </summary>
        public void VerifyCanLoad()
        {
            try
            {
                string sql = "SELECT Count(*) From CommandJournal";
                var connection = GetConnection();
                using (connection)
                {
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = sql;
                    connection.Open();
                    cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("cant restore database from storage", ex);
            }
        }

        public Model GetMostRecentSnapshot(out long lastEntryId)
        {
            lastEntryId = -1;
            string sql = "select id, path from Snapshots order by id desc";
            var conn = GetConnection();
            using (conn)
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    lastEntryId = id;
                    string path = reader.GetString(1);
                    if(File.Exists(path))
                    {
                        
                        using (var stream = File.OpenRead(path))
                        {
                            return _config.CreateSerializer().Read<Model>(stream);

                        }
                        
                    }
                }
            }
            return null;
        }

        public void WriteSnapshot(Model model, string name)
        {
            string path = Path.Combine(_config.SnapshotLocation, name + ".snapshot");
            var stream = File.OpenWrite(path);
            using (stream)
            {
                _config.CreateSerializer().Write(model, stream);
            }
            
            //add a row in the database
            string sql = String.Format("INSERT Snapshots VALUES({0}, getdate(), '{1}')", 42, path);
            var connection = GetConnection();
            using (connection)
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Database exists, but tables dont
        /// </summary>
        public void VerifyCanCreate()
        {
            
        }

        public void Initialize(Model model)
        {
            
        }

        public bool Exists
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool CanCreate
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
