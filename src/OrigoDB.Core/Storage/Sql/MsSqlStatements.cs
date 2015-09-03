using System.Text;

namespace OrigoDB.Core.Storage.Sql
{
    public class MsSqlStatements : SqlStatements
    {
        public MsSqlStatements()
        {
            ReadEntries = "SELECT Id, Created, Data FROM {0} WHERE Id >= @id ORDER BY Id";
            InitStore = BuildInitStatement();
            AppendEntry = "INSERT INTO {0} VALUES (@Id, @Created, @Type, @Data);";
        }

        private string BuildInitStatement()
        {
            var sb = new StringBuilder();
            sb.Append("IF NOT EXISTS ( SELECT * FROM sys.tables WHERE name = '{0}')\n");
            sb.Append("CREATE TABLE {0}\n");
            sb.AppendLine("(");
            sb.AppendLine("   Id bigint not null primary key,");
            sb.AppendLine("   Created datetime not null,");
            sb.AppendLine("   Type varchar(1024) not null,");
            sb.AppendLine("   Data varbinary(max) not null);");
            return sb.ToString();
        }
    }
}