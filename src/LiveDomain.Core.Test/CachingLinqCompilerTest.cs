using LiveDomain.Core.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace LiveDomain.Core.Test
{


    /// <summary>
    ///This is a test class for CachingLinqCompilerTest and is intended
    ///to contain all CachingLinqCompilerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CachingLinqCompilerTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for CachingLinqCompiler Constructor
        ///</summary>
        [TestMethod()]
        public void CanCreateCompilerInstance()
        {
            new CachingLinqCompiler<TestModel>();
        }

        [TestMethod()]
        public void CanCompileValidQueries()
        {
            int failedQueries = 0;
            for (int i = 0; i < validQueries.Length;i++)
            {
                QueryDefinition queryDefinition = validQueries[i];
                try
                {
                    CachingLinqCompiler<TestModel> target = new CachingLinqCompiler<TestModel>();
                    var methodInfo = target.GetCompiledQuery(queryDefinition.Query, queryDefinition.Args);
                }
                catch (Exception ex)
                {
                    failedQueries++;
                    Console.WriteLine("Query id {0} failed", i);
                    Console.WriteLine(ex);
                }
            }

            Assert.AreEqual(0, failedQueries);

        }

        [TestMethod]
        public void RepeatedQueryIsCached()
        {
            CachingLinqCompiler<TestModel> target = new CachingLinqCompiler<TestModel>();
            var queryDefinition = validQueries[0];
            Assert.AreEqual(0, target.CompilerInvocations);
            target.GetCompiledQuery(queryDefinition.Query, queryDefinition.Args);
            Assert.AreEqual(1, target.CompilerInvocations);
            target.GetCompiledQuery(queryDefinition.Query, queryDefinition.Args);
            Assert.AreEqual(1, target.CompilerInvocations);
        }

        [TestMethod]
        public void RepeatedQueryIsCachedWhenParametersDiffer()
        {
            CachingLinqCompiler<TestModel> target = new CachingLinqCompiler<TestModel>();
            var queryDefinition = validQueries[0];
            Assert.AreEqual(0, target.CompilerInvocations);
            target.GetCompiledQuery(queryDefinition.Query, new object[] { "H" });
            Assert.AreEqual(1, target.CompilerInvocations);
            target.GetCompiledQuery(queryDefinition.Query, new object[]{"R"});
            Assert.AreEqual(1, target.CompilerInvocations);
        }

        [TestMethod]
        public void RepeatedQueryIsCompiledWhenCompilationIsForced()
        {
            CachingLinqCompiler<TestModel> target = new CachingLinqCompiler<TestModel>();
            target.ForceCompilation = true;
            var queryDefinition = validQueries[0];
            Assert.AreEqual(0, target.CompilerInvocations);
            target.GetCompiledQuery(queryDefinition.Query, queryDefinition.Args);
            Assert.AreEqual(1, target.CompilerInvocations);
            target.GetCompiledQuery(queryDefinition.Query, queryDefinition.Args);
            Assert.AreEqual(2, target.CompilerInvocations);
        }

        [TestMethod]
        public void CanExecuteQueryWithNewDifferentParameters()
        {
            string expected0 = "Homer Simpson";
            string expected1 = "Robert Friberg";

            var queryDef = validQueries[0];
            Engine<TestModel> engine = Engine.LoadOrCreate<TestModel>();

            //mutate model using a lambda, Dont ever do this unless testing, modification will not be journaled!
            engine.Execute(m =>
            {
                m.AddCustomer(expected0);
                return 1;
            });
            engine.Execute(m =>
            {
                m.AddCustomer(expected1);
                return 1;
            });
           
            string actual = engine.Execute<TestModel, string>(queryDef.Query, "Ho");
            Assert.AreEqual(expected0, actual);
            actual = (string) engine.Execute(queryDef.Query, "Ro");
            Assert.AreEqual(actual, expected1);
        }

        [TestMethod]
        public void CanExecuteValidQueries()
        {
            string expected = "Homer Simpson";

            var queryDef = validQueries[0];
            Engine<TestModel> engine = Engine.LoadOrCreate<TestModel>();

            //mutate model using a lambda, Dont ever do this unless testing, modification will not be journaled!
            engine.Execute(m => { m.AddCustomer(expected);
                                          return 1;
            });

            
            string actual = engine.Execute<TestModel, string>(queryDef.Query, queryDef.Args);
            Assert.AreEqual(expected, actual);
        }

        private QueryDefinition[] validQueries = new QueryDefinition[]
                                                     {
                                                         new QueryDefinition
                                                             {
                                                                 Query = @"
(from customer in db.Customers
where customer.Name.StartsWith(@arg0)
orderby customer.Name
select customer.Name)
.First()",
                              Args = new object[]{"H"}

                                                             }
                                                     };

        class QueryDefinition
        {
            public string Query;
            public object[] Args;
        }
    }
}
