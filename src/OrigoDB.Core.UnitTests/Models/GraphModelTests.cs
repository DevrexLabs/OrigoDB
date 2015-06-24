using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using OrigoDB.Core.Models;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class GraphStoreTests
    {
        [Test]
        public void SmokeTest()
        {
            var config = EngineConfiguration.Create().ForIsolatedTest();
            var store = Db.For<GraphStore>(config);
            var user1 = store.CreateNode("user");
            var user2 = store.CreateNode("user");
            var tweet = store.CreateNode("tweet");
            store.CreateEdge(user1, tweet, "tweeted");
            store.CreateEdge(user2, tweet, "retweeted");
            store.CreateEdge(user1, user2, "followed");
            store.CreateEdge(user1, user1, "followed");

            //how many users have at least one tweet?
            Expression<Func<GraphStore, int>> query = 
                g => g.Nodes.Count(n => n.Label == "user" && n.Out.Any(e => e.Label == "tweeted"));
            var result = store.Query(query);
            Assert.AreEqual(1, result);

            //What is the maximum number of retweets for any tweet
            query = g => g.Nodes
                .Where(n => n.Label == "tweet")
                .Max(n => n.In.Count(e => e.Label == "retweeted"));
            result = store.Query(query);
            Assert.AreEqual(1, result);

            //nodes having self references
            Expression<Func<GraphStore, IEnumerable<GraphStore.Node>>> query2 =
                g => g.Nodes.Where(n => n.Out.Any(e => n.In.Contains(e)));

            var node = store.Query(query2).Single();
            Assert.AreEqual(node.Id, user1);
        }
    }
}