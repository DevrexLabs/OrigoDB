using System;
using System.Data;
using System.Data.Common;
using NUnit.Framework;

namespace OrigoDB.Core.Test
{
    
    
    /// <summary>
    ///This is a test class for SqlStorageTest and is intended
    ///to contain all SqlStorageTest Unit Tests
    ///</summary>
    [TestFixture]
    public class SqlStorageTest
    {



        [Test]
        public void CanLoadSqlProviderFactory()
        {
            var dataTable = DbProviderFactories.GetFactoryClasses();
            foreach (DataColumn column in dataTable.Columns)
            {
                Console.WriteLine(column.ColumnName);
            }
            Console.WriteLine("------------------------------------------------------");
            foreach (DataRow row in dataTable.Rows)
            {

                foreach (object item in row.ItemArray)
                {
                    Console.WriteLine(item);
                }
                Console.WriteLine("---------------------------------------------------");
            }

            var factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
            Assert.IsNotNull(factory);
        }
    }
}
