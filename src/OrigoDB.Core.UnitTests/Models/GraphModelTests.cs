using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using OrigoDB.Core.Models;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class GraphModelTests
    {
        private GraphModel _graph;
        private long _user1;

        [SetUp]
        public void Init()
        {
            _graph = new GraphModel();
            _user1 = _graph.CreateNode("user");
            var user2 = _graph.CreateNode("user");
            var tweet = _graph.CreateNode("tweet");
            _graph.CreateEdge(_user1, tweet, "tweeted");
            _graph.CreateEdge(user2, tweet, "retweeted");
            _graph.CreateEdge(_user1, user2, "followed");
            _graph.CreateEdge(_user1, _user1, "followed");            
        }
        [Test]
        public void GraphIsSerializable()
        {
            new BinaryFormatter().Clone(_graph);
        }

        [Test]
        public void Queries()
        {
            //how many users have at least one tweet?
            Expression<Func<GraphModel, int>> query = 
                g => g.Nodes.Count(n => n.Label == "user" && n.Out.Any(e => e.Label == "tweeted"));
            var result = _graph.Query(query);
            Assert.AreEqual(1, result);

            //What is the maximum number of retweets for any tweet
            query = g => g.Nodes
                .Where(n => n.Label == "tweet")
                .Max(n => n.In.Count(e => e.Label == "retweeted"));
            result = _graph.Query(query);
            Assert.AreEqual(1, result);

            //nodes having self references
            Expression<Func<GraphModel, IEnumerable<GraphModel.Node>>> query2 =
                g => g.Nodes.Where(n => n.Out.Any(e => n.In.Contains(e)));

            var node = _graph.Query(query2).Single();
            Assert.AreEqual(node.Id, _user1);
        }
    }
}