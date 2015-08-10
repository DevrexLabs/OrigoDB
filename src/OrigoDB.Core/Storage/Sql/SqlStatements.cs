namespace OrigoDB.Core.Storage.Sql
{
    public class SqlStatements
    {
        private readonly string _tableName;

        public SqlStatements(string tableName)
        {
            _tableName = tableName;
        }

        public virtual string CreateTableStatement()
        {
            return "CREATE TABLE " + _tableName +
                   "(\n" +
                   "   Id bigint not null primary key,\n" +
                   "   Created not null datetime,\n" +
                   "   Type not null varchar(1024),\n" +
                   "   Data varbinary(max) not null" +
                   ");";
        }

        public virtual string AppendEntryStatement()
        {
            return "INSERT INTO " + _tableName + " VALUES (@Id, @Created, @Type, @Data);";
        }
    }
}