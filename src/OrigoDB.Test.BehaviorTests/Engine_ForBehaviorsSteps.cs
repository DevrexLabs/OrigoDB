using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OrigoDB.Core;
using OrigoDB.Test.Common;
using TechTalk.SpecFlow;
using TC = OrigoDB.Test.Common.TestContext;

namespace OrigoDB.Test.BehaviorTests
{
    [Binding]
    public class Engine_ForBehaviorsSteps
    {
        [Given(@"a generic model")]
        public void GivenAGenericModel()
        {
            var genericModel = new GenericModel<Entity>();
            TC.Bag.GenericModel = genericModel;
        }
        
        [Given(@"an Engine for that model")]
        public void GivenAnEngineForThatModel()
        {
            //var config = EngineConfiguration.Create().ForIsolatedTest();
            //config.PersistenceMode = PersistenceMode.ManualSnapshots;
            var engine = Engine.For<GenericModel<Entity>>();//(config);
            TC.Bag.Engine = engine;
        }
        
        [Given(@"a command")]
        public void GivenACommand()
        {
            var entity = new Entity {Name = "Ordinary Entity"};
            TC.Bag.Entity = entity;
            var command = new InsertCommand<Entity>(entity);
            TC.Bag.InsertCommand = command;
        }
        
        [Given(@"a query")]
        public void GivenAQuery()
        {
            var query = new GetByQuery<Entity>(e=>e.Name.StartsWith("O"));
            TC.Bag.GetByQuery = query;
        }
        
        [When(@"executing the command using that engine")]
        public void WhenExecutingTheCommandUsingThatEngine()
        {
            var engine = TC.Bag.Engine as IEngine<GenericModel<Entity>>;
            var command = TC.Bag.InsertCommand as InsertCommand<Entity>;
            engine.Execute(command);
            TC.Bag.InsertedEntityID = (TC.Bag.Entity as Entity).Id;
            TC.Bag.CommandTimeStamp = command.Timestamp;
        }
        [Then(@"the command should work")]
        public void ThenTheCommandShouldWork()
        {
            var entity = TC.Bag.Entity as Entity;
            Assert.That(entity.Id != Guid.NewGuid());
        } 
       
        [When(@"executing the query using that engine")]
        public void WhenExecutingTheQueryUsingThatEngine()
        {
            var engine = TC.Bag.Engine as IEngine<GenericModel<Entity>>;
            var query = TC.Bag.GetByQuery as GetByQuery<Entity>;
            var results = engine.Execute(query);
            TC.Bag.QueryResults = results;
        }
        

        
        [Then(@"the query should work")]
        public void ThenTheQueryShouldWork()
        {
            var results = TC.Bag.QueryResults as IEnumerable<Entity>;
            //Assert.That(results.Count() == 1);
            var retrievedEntity = results.FirstOrDefault();
            Assert.That(TC.Bag.InsertedEntityID == retrievedEntity.Id);
        }
    }
}