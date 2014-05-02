using System;
using System.Runtime.Serialization.Formatters.Binary;
using OrigoDB.Core.Migrations;
using NUnit.Framework;

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

        private BinaryFormatter _formatter;

        [SetUp]
        public void Setup()
        {
            Schema.Current.TypeSubstitutions.Clear();
            _formatter = new BinaryFormatter();
            _formatter.Binder = new CustomBinder(Schema.Current);
        }



        [Test]
        public void CanRenameType()
        {
            Schema
                .Substitute(typeof(MigrationTestTypeA).FullName)
                .With<MigrationTestTypeB>();

            var bytes = _formatter.ToByteArray(new MigrationTestTypeA());
            var actual = _formatter.FromByteArray<object>(bytes).GetType();
            var expected = typeof(MigrationTestTypeB);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanRenameGenericTypeParameterSpecifically()
        {
            Schema
                .Substitute(typeof(GenericDummy<MigrationTestTypeA, MigrationTestTypeA>).FullName)
                .With<GenericDummy<MigrationTestTypeB, MigrationTestTypeB>>();

            var bytes = _formatter.ToByteArray(new GenericDummy<MigrationTestTypeA, MigrationTestTypeA>());
            var actual = _formatter.FromByteArray<object>(bytes).GetType();
            var expected = typeof(GenericDummy<MigrationTestTypeB, MigrationTestTypeB>);
            Assert.AreEqual(expected, actual);
        }

        [Test, Ignore]
        public void CanRenameGenericTypeParameterType2()
        {
            Schema
                .Substitute(typeof(MigrationTestTypeA).FullName)
                .With<MigrationTestTypeB>();

            var bytes = _formatter.ToByteArray(new GenericDummy<MigrationTestTypeA, MigrationTestTypeA>());
            var actual = _formatter.FromByteArray<object>(bytes).GetType();
            var expected = typeof(GenericDummy<MigrationTestTypeB, MigrationTestTypeB>);
            Assert.AreEqual(expected, actual);            
        }
    }
}