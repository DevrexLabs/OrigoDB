using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OrigoDB.Core.Configuration;

namespace OrigoDB.Core.Test
{
    [Serializable]
    class ImmutableModel : Model
    {
        private readonly List<int> _numbers;

        public ImmutableModel()
            : this(Enumerable.Empty<int>())
        {
            
        }

        public ImmutableModel(IEnumerable<int> numbers)
        {
            _numbers = new List<int>(numbers);
        }

        public IEnumerable<int> Numbers()
        {
            foreach (int n in _numbers) yield return n;
        }

        private IEnumerable<int> WithNumber(int n)
        {
            foreach (var number in Numbers())
            {
                yield return number;
            }
            yield return n;
        }

        public ImmutableModel WithNewNumber(int number)
        {
            return new ImmutableModel(WithNumber(number));
        }
    }

    [Serializable]
    class AppendNumberCommand : ImmutabilityCommand<ImmutableModel>
    {
        public readonly int Number;

        public AppendNumberCommand(int number)
        {
            Number = number;
        }


        public override void Execute(ImmutableModel model, out ImmutableModel result)
        {
            result = model.WithNewNumber(Number);
        }
    }

    [Serializable]
    internal class AppendNumberAndGetSumCommand : ImmutabilityCommand<ImmutableModel, int>
    {
        public readonly int Number;

        public AppendNumberAndGetSumCommand(int number)
        {
            Number = number;
        }
        public override int Execute(ImmutableModel model, out ImmutableModel result)
        {
            result = model.WithNewNumber(Number);
            return result.Numbers().Sum();
        }
    }

    class NumberSumQuery : Query<ImmutableModel, int>
    {
        public override int Execute(ImmutableModel model)
        {
            return model.Numbers().Sum();
        }
    }


    [TestFixture]
    public class ImmutabilityTests
    {
        [Test]
        public void ImmutabilityKernelSmokeTest()
        {
            var config = EngineConfiguration.Create().WithImmutability();
            var model = new ImmutableModel();
            var  kernel = new ImmutabilityKernel(config, model);

            int actual = (int) kernel.ExecuteCommand(new AppendNumberAndGetSumCommand(42));
            Assert.AreEqual(42, actual);
            kernel.ExecuteCommand(new AppendNumberCommand(58));
            actual = kernel.ExecuteQuery(new NumberSumQuery());
            Assert.AreEqual(actual, 42 + 58);
        }

        [Test]
        public void ImmutabilityEngineSmokeTest()
        {
            var config = EngineConfiguration.Create().WithImmutability();
            config.SetStoreFactory(cfg => new InMemoryStore(cfg));
            
            var engine = Engine.For<ImmutableModel>(config);

            int actual = engine.Execute(new AppendNumberAndGetSumCommand(42));
            Assert.AreEqual(42, actual);
            engine.Execute(new AppendNumberCommand(58));
            actual = engine.Execute(new NumberSumQuery());
            Assert.AreEqual(actual, 42 + 58);
        }

        [Test]
        public void Engine_self_configures_for_immutability_model()
        {
            var config = EngineConfiguration.Create().WithImmutability();
            Assert.AreEqual(Kernels.Immutability, config.Kernel);
            Assert.AreEqual(SynchronizationMode.None, config.Synchronization);
        }
    }
}