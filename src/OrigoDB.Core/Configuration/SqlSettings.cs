using System.Configuration;

namespace OrigoDB.Core
{

    /// <summary>
    /// Configuration settings when using a backing sql store.
    /// Exposed via EngineConfiguration.SqlSettings property.
    /// </summary>
    public class SqlSettings
    {

        public const string DefaultProviderName = "System.Data.SqlClient";
        public const string DefaultConnectionString = "origo";
        public const string DefaultTableName = "OrigoJournal";

        /// <summary>
        /// Name of db table for journal
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Name of sql provider used to create a DbProviderFactory
        /// See SqlProvider.Providers and DbProviderFactories
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// A connection string or connection string name referencing
        /// connection strings in the application configuration file
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Lookup ConnectionStringSetting in application configuration file using ConnectionString as key.
        /// If it exists, assign ConnectionString and ProviderName properties.
        /// </summary>
        public void ResolveConnectionString()
        {
            var settings = ConfigurationManager.ConnectionStrings[ConnectionString];
            if (settings != null)
            {
                ProviderName = settings.ProviderName;
                ConnectionString = settings.ConnectionString;
            }
        }

        /// <summary>
        /// Set's defaults
        /// </summary>
        public SqlSettings()
        {
            TableName = DefaultTableName;
            ProviderName = DefaultProviderName;
            ConnectionString = DefaultConnectionString;
        }
    }
}
