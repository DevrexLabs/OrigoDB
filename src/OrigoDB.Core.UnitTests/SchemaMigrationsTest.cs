using System;
using System.Runtime.Serialization.Formatters.Binary;
using OrigoDB.Core.Migrations;
using NUnit.Framework;
using System.Collections.Generic;

namespace OrigoDB.Core.UnitTests
{
    [Serializable]
    class MigrationTestTypeA
    {
        public string Foo { get; set; }
    }

    [Serializable]
    class MigrationTestTypeB
    {
        public string Foo { get; set; }
    }

    [Serializable]
    class GenericDummy<T,U>
    {
    }

    [TestFixture]
    public class SchemaMigrationTest
    {
        [SetUp]
        public void Setup()
        {
            Schema.Current.TypeSubstitutions.Clear();
        }

        [Test]
        public void CanRenameType()
        {
            Schema
                .Substitute(typeof(MigrationTestTypeA).FullName)
                .With<MigrationTestTypeB>();

            var serializer = new Serializer(new BinaryFormatter());
            var bytes = serializer.Serialize(new MigrationTestTypeA());
            var actual = serializer.Deserialize<object>(bytes).GetType();
            var expected = typeof(MigrationTestTypeB);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanRenameGenericTypeParameterSpecifically()
        {
            Schema
                .Substitute(typeof(GenericDummy<MigrationTestTypeA, MigrationTestTypeA>).FullName)
                .With<GenericDummy<MigrationTestTypeB, MigrationTestTypeB>>();

            var serializer = new Serializer(new BinaryFormatter());
            var bytes = serializer.Serialize(new GenericDummy<MigrationTestTypeA, MigrationTestTypeA>());
            var actual = serializer.Deserialize<object>(bytes).GetType();
            var expected = typeof(GenericDummy<MigrationTestTypeB, MigrationTestTypeB>);
            Assert.AreEqual(expected, actual);
        }

        [Test, Ignore]
        public void CanRenameGenericTypeParameterType2()
        {
            Schema
                .Substitute(typeof(MigrationTestTypeA).FullName)
                .With<MigrationTestTypeB>();

            var serializer = new Serializer(new BinaryFormatter());
            var bytes = serializer.Serialize(new GenericDummy<MigrationTestTypeA, MigrationTestTypeA>());
            var actual = serializer.Deserialize<object>(bytes).GetType();
            var expected = typeof(GenericDummy<MigrationTestTypeB, MigrationTestTypeB>);
            Assert.AreEqual(expected, actual);            
        }
    }
}