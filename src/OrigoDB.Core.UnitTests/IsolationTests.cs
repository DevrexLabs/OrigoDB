using System;
using System.Collections.Generic;
using System.Messaging;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class IsolationTests
    {

        [Immutable, Serializable]
        public class MyImmutable
        {
        }

        [Serializable]
        public class Foo
        {
            
        }

        [Isolation(IsolationLevel.Input), Serializable]
        public class IsolationLevelInput: Command<Model>
        {
            public override void Execute(Model model)
            {
                throw new NotImplementedException();
            }
        }

        [Serializable, Isolation(IsolationLevel.Output)]
        public class IsolationLevelOutput : Command<Model>
        {

            public override void Execute(Model model)
            {
                throw new NotImplementedException();
            }
        }

        [Serializable, Isolation(IsolationLevel.InputOutput)]
        public class IsolationLevelInputOutput : Command<Model>
        {
            public override void Execute(Model model)
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void Command_with_isolationlevel_input_not_cloned()
        {
            var cmd = new IsolationLevelInput();
            var cmd2 = ApplyStrategy(cmd);
            Assert.AreEqual(cmd,cmd2);
        }

        [Test]
        public void Command_with_isolationlevel_inputoutput_not_cloned()
        {
            var cmd = new IsolationLevelInputOutput();
            var cmd2 = ApplyStrategy(cmd);
            Assert.AreEqual(cmd, cmd2);            
        }

        [Test]
        public void Command_with_isolationlevel_output_is_cloned()
        {
            var cmd = new IsolationLevelOutput();
            var cmd2 = ApplyStrategy(cmd);
            Assert.AreNotEqual(cmd, cmd2);           
        }

        [Test]
        public void Result_marked_immutable_not_cloned()
        {
            var result = new MyImmutable();
            var result2 = ApplyStrategy(result, new object());
            Assert.AreEqual(result, result2);
        }

        private IEnumerable<object> KnownTypeObjects()
        {
            yield return new DateTime();
            yield return "Fish heads";
            yield return 42;
            yield return TimeSpan.FromSeconds(42);
            yield return new object();
            yield return Guid.Parse("CD81FFCF-58BC-4249-9521-237F60B33D1B");
        }

        [Test, TestCaseSource("KnownTypeObjects")]
        public void Known_type_not_cloned(object result)
        {
            var result2 = ApplyStrategy(result, new object());
            Assert.IsTrue(ReferenceEquals(result,result2));
        }

        [Test]
        public void Type_registered_as_isolated_is_not_cloned()
        {
            var isolated = new Foo();
            var isolated2 = ApplyStrategy(isolated, new object());
            Assert.AreNotEqual(isolated,isolated2);
            IsolatedReturnTypes.Add(typeof(Foo));
            isolated2 = ApplyStrategy(isolated, new object());
            Assert.AreEqual(isolated,isolated2);
        }

        [Test]
        public void Mutable_reference_type_not_cloned_when_producer_has_isolation_level_output()
        {
            var mutable = new Foo();
            var mutable2 = ApplyStrategy(mutable, new IsolationLevelOutput());
            Assert.AreEqual(mutable, mutable2);
        }

        [Test]
        public void Mutable_reference_type_not_cloned_when_producer_has_isolation_level_inputoutput()
        {
            var mutable = new Foo();
            var mutable2 = ApplyStrategy(mutable, new IsolationLevelInputOutput());
            Assert.AreEqual(mutable, mutable2);
        }

        [Test]
        public void Mutable_reference_type_cloned_when_producer_has_isolation_level_input()
        {
            var mutable = new Foo();
            var mutable2 = ApplyStrategy(mutable, new IsolationLevelInput());
            Assert.AreNotEqual(mutable, mutable2);
        }

        private Command ApplyStrategy(Command command)
        {
            var strategy = new HeuristicCloneStrategy();
            strategy.SetFormatter(new BinaryFormatter());
            strategy.Apply(ref command);
            return command;
        }

        private T ApplyStrategy<T>(T result, object producer)
        {
            var strategy = new HeuristicCloneStrategy();
            strategy.SetFormatter(new BinaryFormatter());
            object o = result;
            strategy.Apply(ref o, producer);
            return (T)o;          
        }
    }
}