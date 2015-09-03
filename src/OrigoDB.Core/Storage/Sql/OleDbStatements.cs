using System.Text;

namespace OrigoDB.Core.Storage.Sql
{
    /// <summary>
    /// Not yet tested! Please let us know if try it out
    /// </summary>
    public class OleDbStatements : SqlStatements
    {
        public OleDbStatements()
        {
            ReadEntries = "SELECT Id, Created, Data FROM {0} WHERE Id >= ? ORDER BY ID";
            InitStore = BuildInitStatement();
            AppendEntry = "INSERT INTO {0} VALUES (?,?,?,?);";
        }
        private string BuildInitStatement()
        {
            var sb = new StringBuilder();
            sb.Append("IF NOT EXISTS ( SELECT * FROM INFORMATION.TABLES WHERE TABLE_NAME = '{0}')\n");
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