using System.Configuration;
using System.Text;

namespace OrigoDB.Core.Storage.Sql
{
    /// <summary>
    /// Provider for Microsoft Sql Server. Tested with MS 2014 Developer Edition
    /// </summary>
    public class MsSqlProvider : SqlProvider
    {
        /// <summary>
        /// Name of provider as recognized by DataProviderFactories.
        /// </summary>
        public const string Name = "System.Data.SqlClient";


        public MsSqlProvider(SqlSettings config)
            : base(config)
        {
            ReadEntriesStatement = "SELECT Id, Created, Data FROM [" + TableName + "] WHERE Id  >= @id ORDER BY Id";
            InitStatement = BuildInitStatement();
            AppendEntryStatement = "INSERT INTO [" + TableName + "] VALUES (@Id, @Created, @Type, @Data);";
        }

        private string BuildInitStatement()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("IF NOT EXISTS ( SELECT * FROM sys.tables WHERE name = '{0}')\n", TableName);
            sb.AppendFormat("CREATE TABLE {0}\n", TableName);
            sb.AppendLine("(");
            sb.AppendLine("   Id bigint not null primary key,");
            sb.AppendLine("   Created datetime not null,");
            sb.AppendLine("   Type varchar(1024) not null,");
            sb.AppendLine("   Data varbinary(max) not null);");
            return sb.ToString();
        }
    }
}