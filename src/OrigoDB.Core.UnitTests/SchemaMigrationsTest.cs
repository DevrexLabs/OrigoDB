using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using NUnit.Framework;

namespace OrigoDB.Core.Test
{
    [Serializable]
    class TypeA
    {
        public string Foo { get; set; }
    }

    [Serializable]
    class TypeB
    {
        public string Foo { get; set; }
    }

    [Serializable]
    class NestedA
    {
        public TypeA A { get; set; }
    }

    [Serializable]
    class NestedB
    {
        public TypeB B { get; set; }
    }

    [Serializable]
    class GenericDummy<T1, T2>
    {
    }

    [TestFixture]
    public class SchemaMigrationTest
    {

        private IFormatter _formatter;

        [SetUp]
        public void Setup()
        {
            var substitutions = new Dictionary<string, Type>();
            substitutions[typeof (TypeA).FullName] = typeof (TypeB);
            substitutions[typeof (GenericDummy<TypeA, TypeA>).FullName] =
                typeof (GenericDummy<TypeB, TypeB>);
            substitutions[typeof (NestedA).FullName] = typeof (NestedB);
            var config = new EngineConfiguration();
            config.SetSerializationTypeMappings(substitutions);
            _formatter = config.CreateFormatter(FormatterUsage.Default);
        }



        [Test]
        public void CanRenameSimpleType()
        {
            var bytes = _formatter.ToByteArray(new TypeA());
            var actual = _formatter.FromByteArray<object>(bytes).GetType();
            var expected = typeof(TypeB);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanRenameGenericTypeParameters()
        {
            var bytes = _formatter.ToByteArray(new GenericDummy<TypeA, TypeA>());
            var actual = _formatter.FromByteArray<object>(bytes).GetType();
            var expected = typeof(GenericDummy<TypeB, TypeB>);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanRenameNestedTypes()
        {
            var bytes = _formatter.ToByteArray(new NestedA{A = new TypeA()});
            var actual = _formatter.FromByteArray<object>(bytes).GetType();
            var expected = typeof(NestedB);
            Assert.AreEqual(expected, actual);
               
        }
    }
}