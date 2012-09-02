using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core;

namespace LiveDomain.Modules.SqlStorage
{

    public enum LocationType
    {
        /// <summary>
        /// EngineConfiguration.Location is the name of a connection string in the application configuration file
        /// </summary>
        ConnectionStringName,

        /// <summary>
        /// EngineConfiguration.Location is a connection string, 
        /// Microsoft Sql Server provider is assumed, otherwise ProviderName property must also be set
        /// </summary>
        ConnectionString
    }

    public class SqlEngineConfiguration : EngineConfiguration
    {

        public const string DefaultJournalTableName = "CommandJournal";
        
        public string JournalTableName { get; set; }

        /// <summary>
        /// Decides how EngineConfiguration.Location is interpreted. As a connection string or connection string name.
        /// If connection string, then you may also need to set the ProviderName property
        /// </summary>
        public LocationType LocationType { get; set; }


        public string ProviderName { get; set; }

        public SqlEngineConfiguration()
        {
            LocationType = LocationType.ConnectionStringName;
            ProviderName = "System.Data.SqlClient";
            JournalTableName = DefaultJournalTableName;
            base.SetStoreFactory(c => new SqlStore((SqlEngineConfiguration)c));
        }
 
        public SqlEngineConfiguration(string connectionStringName) : base(connectionStringName)
        {
        }
    }
}
