namespace OrigoDB.Core.Storage.Sql
{
    /// <summary>
    /// The sql statements to use with a specific Net Data Provider Factory
    /// Used with String.Format to inject table name from SqlSettings
    /// </summary>
    public class SqlStatements
    {

        /// <summary>
        /// A parameterized statement compatible with the spe
        /// </summary>
        public string ReadEntries { get; set; }
        public string AppendEntry { get; set; }
        public string InitStore { get; set; }
        public string TruncateEntries { get; set; }
    }
}