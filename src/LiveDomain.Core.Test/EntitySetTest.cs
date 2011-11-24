using LiveDomain.Relational;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace LiveDomain.Core.Test
{
    
    
    /// <summary>
    ///This is a test class for EntitySetTest and is intended
    ///to contain all EntitySetTest Unit Tests
    ///</summary>
    [TestClass()]
    public class EntitySetTest
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
public void can_add_entity()
{
    var engine = Engine.LoadOrCreate<MyRelationalModel>();
            
    //Create the entity you want to insert
    var category = new Category{ Name = "Beverages"};
            
    //Create a generic command
    var addCommand = new AddEntityCommand<Category>(category);

    //A copy is returned with the the Id assigned
    category = (Category) engine.Execute(addCommand);
    int numEntities = engine.Execute( db => db.SetOf<Category>().Count());
    Assert.IsTrue(numEntities == 1);
    engine.Close();
}


[TestMethod()]
public void added_entity_was_assigned_a_key()
{
    var engine = Engine.LoadOrCreate<MyRelationalModel>();
    var category = new Category { Name = "Beverages" };
    var addCommand = new AddEntityCommand<Category>(category);
    category = (Category)engine.Execute(addCommand);
    Assert.IsTrue(category.Id > 0);
    engine.Close();
}
}
}
