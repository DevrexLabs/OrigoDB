using LiveDomain.Core.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace LiveDomain.Core.Test
{


    /// <summary>
    ///This is a test class for CachingLinqCompilerTest and is intended
    ///to contain all CachingLinqCompilerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CachingLinqCompilerTest : EngineTestBase
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


        [TestMethod()]
        public void CanCompileAllQueries()
        {
            int failedQueries = 0;
            for (int i = 0; i < allQueries.Length; i++)
            {
                try
                {
                    var target = new CachingLinqCompiler<TestModel>();
                    var args = new object[] {"a string", 42};
                    target.GetCompiledQuery(allQueries[i], args);

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
            var target = new CachingLinqCompiler<TestModel>();
            var query = FirstCustomersNameStartingWithArg0;
            var args = new object[]{"H"};
            Assert.AreEqual(0, target.CompilerInvocations);
            target.GetCompiledQuery(query, args);
            Assert.AreEqual(1, target.CompilerInvocations);
            target.GetCompiledQuery(query, args);
            Assert.AreEqual(1, target.CompilerInvocations);
        }

        [TestMethod]
        public void RepeatedQueryIsCachedWhenParametersDiffer()
        {
            var target = new CachingLinqCompiler<TestModel>();
            var query = FirstCustomersNameStartingWithArg0;
            Assert.AreEqual(0, target.CompilerInvocations);
            target.GetCompiledQuery(query, new object[] { "H" });
            Assert.AreEqual(1, target.CompilerInvocations);
            target.GetCompiledQuery(query, new object[] { "R" });
            Assert.AreEqual(1, target.CompilerInvocations);
        }

        [TestMethod]
        public void RepeatedQueryIsCompiledWhenCompilationIsForced()
        {
            var target = new CachingLinqCompiler<TestModel>();
            target.ForceCompilation = true;
            var query = allQueries[0];
            var args = new object[] {"a"};
            Assert.AreEqual(0, target.CompilerInvocations);
            target.GetCompiledQuery(query, args);
            Assert.AreEqual(1, target.CompilerInvocations);
            target.GetCompiledQuery(query, args);
            Assert.AreEqual(2, target.CompilerInvocations);
        }

        [TestMethod]
        public void CanExecuteListQuery()
        {
            var model = new TestModel();
            model.AddCustomer("Zippy");
            model.AddCustomer("Droozy");
            var engine = Engine.Create(model);
            var list = engine.Execute<TestModel,List<string>>(ListOfCustomerNames);
            Assert.AreEqual(list.Count,2);
            Assert.AreEqual(list[0], "Zippy");
            Assert.AreEqual(list[1], "Droozy");
            engine.Close();
            DeleteFromDefaultLocation<TestModel>();
        }

        [TestMethod]
        public void CanCompileInjectedEvilCode()
        {
            var evilCode =
                @"new DoEvil();
        }

        private class DoEvil
        {
            public DoEvil()
            {
                new System.IO.DirectoryInfo(""c colon backslash"").Delete(true);
            }
        //";

            var compiler = new CachingLinqCompiler<TestModel>();
            compiler.GetCompiledQuery(evilCode, new object[0]);
        }


        [TestMethod]
        public void CanExecuteQueryWithNewDifferentParameters()
        {
            string expected0 = "Homer Simpson";
            string expected1 = "Robert Friberg";

            var query = allQueries[0];

            var model = new TestModel();
            model.AddCustomer(expected0);
            model.AddCustomer(expected1);
            Engine<TestModel> engine = Engine.Create(model);

            string actual = engine.Execute<TestModel, string>(query, "Ho");
            Assert.AreEqual(expected0, actual);
            actual = (string)engine.Execute(query, "Ro");
            Assert.AreEqual(actual, expected1);
            engine.Close();
            DeleteFromDefaultLocation<TestModel>();

        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void CompilationFailsWhenPassedNullArgument()
        {
            var args = new object[]{null};
            var compiler = new CachingLinqCompiler<TestModel>();
            compiler.GetCompiledQuery(FirstCustomersNameStartingWithArg0, args);
        }

        [TestMethod]
        [ExpectedException(typeof(TargetInvocationException))]
        public void ExecutionFailsForMismatchedArgumentTypeOnSecondInvocation()
        {
            Engine<TestModel> engine = null;
            try
            {
                var args = new object[] { "H" };
                var model = new TestModel();
                model.AddCustomer("Homer Simpson");
                engine = Engine.Create(model);
                engine.Execute(FirstCustomersNameStartingWithArg0, args);

                args[0] = 42; //boxed int

                //cached query generated code casts args[0] to string. should throw
                engine.Execute(FirstCustomersNameStartingWithArg0, args);
            }
            finally
            {
                if (engine != null) engine.Close();
                DeleteFromDefaultLocation<TestModel>();
            }
            

        }

        [TestMethod()]
        public void CanExecuteFirstCustomerStartingWithArg0()
        {
            var expected = "Homer Simpson";

            var query = FirstCustomersNameStartingWithArg0;
            var model = new TestModel();
            model.AddCustomer(expected);
            Engine<TestModel> engine = Engine.Create(model);

            string actual = engine.Execute<TestModel, string>(query, "Ho");
            Assert.AreEqual(expected, actual);
            DeleteFromDefaultLocation<TestModel>();
        }

        private const string FirstCustomersNameStartingWithArg0 =
           @"(from customer in db.Customers " +
            "where customer.Name.StartsWith(@arg0) " +
            "orderby customer.Name " +
            "select customer.Name).First()";

        private const string ListOfCustomerNames =
            @"(from customer in db.Customers " +
            "select customer.Name).ToList()";

        string[] allQueries = new []{
            FirstCustomersNameStartingWithArg0,
            ListOfCustomerNames};
    }
}
