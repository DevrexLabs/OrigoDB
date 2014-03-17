using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using OrigoDB.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;
using ProtoBuf.Meta;

namespace Modules.ProtoBuf.Test
{
    [Serializable]
    internal class MyModel : Model
    {
    }


    public class ClassWithoutAnyFields
    {

    }

    public class ClassWithAReadonlyField
    {
#pragma warning disable 169
        private readonly int X;
#pragma warning restore 169
   }

    public class ClassWithPrivateRegularField
    {
#pragma warning disable 169
        private int x;
#pragma warning restore 169
    }

    public class EmptyClassWithParameterizedConstructor
    {

        public EmptyClassWithParameterizedConstructor(int x)
        {

        }
    }

    internal class ClassWithOnePublicReadonlyFieldAndMatchingParameterizedConstructor
    {
#pragma warning disable 649
        public readonly int X;
#pragma warning restore 649
        public ClassWithOnePublicReadonlyFieldAndMatchingParameterizedConstructor(int x)
        {

        }
    }

    /// <summary>
    /// A class with public readonly fields and a public constructor 
    /// with parameters with matching types and names!
    /// </summary>
    class FailingType
    {
        public readonly int MyField;
        public readonly string MyField2;

        public FailingType(int myField, string myField2)
        {
            MyField = myField;
            MyField2 = myField2;

        }
    }

    internal class MyMessage3 : Command<MyModel>
    {
        public readonly int MyField;
        public readonly string MyField2;

        public MyMessage3(int myField, string myField2)
        {
            MyField = myField;
            MyField2 = myField2;

        }

        //public MyMessage()
        //{
        //    MyField = 18;
        //}

        public override void Execute(MyModel model)
        {

        }
    }

    class HelloProto
    {
        public readonly int MyField = 45;
        protected string wtf = "dog eat dog";
        public string MyProperty { get; set; }

        public HelloProto(int x)
        {
            x *= x;
        }
    }

    class HelloSubProto : HelloProto
    {
        public string MySubProperty { get; set; }

        public HelloSubProto(int x)
            : base(x)
        {

        }
    }

    [TestClass]
    public class SmokeTests
    {

        private class MetaTypeInfo
        {
            public MetaType MetaType;
            public int NumFields;
        }

        private bool IsDefined(RuntimeTypeModel typeModel, Type theSoughtType)
        {
            foreach (MetaType aDefinedMetaType in typeModel.GetTypes())
            {
                if (theSoughtType == aDefinedMetaType.Type) return true;
            }
            return false;

        }

        private void Add<T>(RuntimeTypeModel typeModel)
        {
            Type type = typeof(T);
            var inheritanceChain = new Stack<MetaTypeInfo>();
            while (type != typeof(object) && type != typeof(ValueType))
            {

                if (IsDefined(typeModel, type))
                {
                    int fieldCount = FieldNames(type, false).Length;
                    var subTypes = typeModel[type].GetSubtypes();
                    if (subTypes != null) fieldCount += subTypes.Length;
                    inheritanceChain.Push(new MetaTypeInfo { NumFields = fieldCount, MetaType = typeModel[type] });
                    break;
                }

                var metaType = typeModel.Add(type, false);
                metaType.UseConstructor = false;
                //metaType.AsReferenceDefault = true;
                int numFields;
                AddInferredFields(metaType, out numFields, includeInherited: false);
                inheritanceChain.Push(new MetaTypeInfo { NumFields = numFields, MetaType = metaType });
                type = type.BaseType;
            }
            var baseType = inheritanceChain.Pop();
            while (inheritanceChain.Count > 0)
            {
                MetaTypeInfo subType = inheritanceChain.Pop();
                baseType.MetaType.AddSubType(baseType.NumFields + 1, subType.MetaType.Type);
                baseType = subType;
            }
        }

        private string[] FieldNames(Type type, bool includeInherited = true)
        {
            var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            if (!includeInherited) flags |= BindingFlags.DeclaredOnly;
            var fieldNames =
                type.GetFields(flags)
                    .Select(fi => fi.Name).ToArray();
            Array.Sort(fieldNames);
            return fieldNames;
        }

        private MetaType AddInferredFields(MetaType metaType, out int numFields, bool includeInherited = true)
        {
            var fieldNames = FieldNames(metaType.Type, includeInherited);
            metaType.Add(fieldNames);
            numFields = fieldNames.Length;
            return metaType;
        }

        private TReturn Clone<TReturn, TRead>(TypeModel typeModel, TRead o, out long bytesWritten)
            where TReturn : TRead
        {
            var memStream = new MemoryStream();
            typeModel.Serialize(memStream, o);
            bytesWritten = memStream.Position;
            memStream.Position = 0;
            return (TReturn)typeModel.Deserialize(memStream, null, typeof(TRead));
        }

        [TestMethod]
        public void no_config()
        {
            var typeModel = RuntimeTypeModel.Create();
            typeModel.AutoAddMissingTypes = true;
            CloneAndCompare(typeModel);

        }

        private long CloneAndCompare(RuntimeTypeModel typeModel)
        {
            DateTime created = DateTime.Now;
            var entry = new JournalEntry<MyMessage3>(42, new MyMessage3(43, "dalmatians"), created);
            long bytesWritten;
            var clonedEntry = (JournalEntry<MyMessage3>)Clone<JournalEntry<MyMessage3>, JournalEntry>(typeModel, entry, out bytesWritten);
            Assert.AreEqual((ulong) 42, clonedEntry.Id);
            Assert.AreEqual(created, clonedEntry.Created);
            Assert.AreEqual(43, clonedEntry.Item.MyField);
            Assert.AreEqual("dalmatians", clonedEntry.Item.MyField2);
            return bytesWritten;
        }

        [TestMethod]
        public void empty_class_with_parameterized_constructor_is_undefined()
        {
            var typeModel = RuntimeTypeModel.Create();
            Assert.IsFalse(typeModel.IsDefined(typeof(EmptyClassWithParameterizedConstructor)));
        }


        [TestMethod]
        public void class_without_fields_is_undefined()
        {
            var typeModel = RuntimeTypeModel.Create();
            Assert.IsFalse(typeModel.IsDefined(typeof(ClassWithoutAnyFields)));
        }

        [TestMethod]
        public void class_with_readonly_field_and_matching_parameterized_constructor_is_undefined()
        {
            var typeModel = RuntimeTypeModel.Create();
            Assert.IsFalse(typeModel.IsDefined(typeof(ClassWithOnePublicReadonlyFieldAndMatchingParameterizedConstructor)));
        }

        [TestMethod]
        public void class_without_private_fields_is_undefined()
        {
            var typeModel = RuntimeTypeModel.Create();
            Assert.IsFalse(typeModel.IsDefined(typeof(ClassWithPrivateRegularField)));
        }

        [TestMethod]
        public void class_with_readonly_fields_is_undefined()
        {
            var typeModel = RuntimeTypeModel.Create();
            Assert.IsFalse(typeModel.IsDefined(typeof(ClassWithAReadonlyField)));
        }


        [TestMethod]
        public void IsDefined_on_empty_model_should_return_false()
        {
            var typeModel = RuntimeTypeModel.Create();
            Assert.IsFalse(typeModel.IsDefined(typeof(FailingType)));
        }

        [TestMethod]
        public void private_inherited_fields_are_handled()
        {

            var typeModel = RuntimeTypeModel.Create();
            Add<HelloSubProto>(typeModel);



            long bytesWritten;
            var subProto = new HelloSubProto(12) { MyProperty = "fish", MySubProperty = "dog" };
            var restored = Clone<HelloSubProto, HelloSubProto>(typeModel, subProto, out bytesWritten);

            Assert.AreEqual(restored.MyField, subProto.MyField);
            Assert.AreEqual(restored.MySubProperty, subProto.MySubProperty);
            Assert.AreEqual(restored.MyProperty, subProto.MyProperty);
            Console.WriteLine("Bytes written: " + bytesWritten);
        }



        [TestMethod]
        public void can_add_type_already_added()
        {
            var model = TypeModel.Create();
            Add<JournalEntry<MyMessage3>>(model);
            Add<JournalEntry<MyMessage3>>(model);
        }

        [TestMethod]
        public void stream_is_same_size_when_type_added_more_than_once()
        {
            var model = TypeModel.Create();
            Add<JournalEntry<MyMessage3>>(model);
            var entry = new JournalEntry<MyMessage3>(42, new MyMessage3(10, "dog"));
            long bytesWrittenSingle;
            Clone<JournalEntry<MyMessage3>, JournalEntry>(model, entry, out bytesWrittenSingle);

            //register same type again, fields should't be added second time around
            Add<JournalEntry<MyMessage3>>(model);
            long bytesWrittenDouble;
            Clone<JournalEntry<MyMessage3>, JournalEntry>(model, entry, out bytesWrittenDouble);

            //if the fields were added to the metatype on the second register
            //output length would have been bigger
            Assert.AreEqual(bytesWrittenSingle, bytesWrittenDouble);
        }

        [TestMethod]
        public void subtype_is_returned_when_basetype_is_requested()
        {
            var model = TypeModel.Create();
            Add<JournalEntry<MyMessage3>>(model);
            Add<MyMessage3>(model);
            CloneAndCompare(model);

        }

        [TestMethod]
        public void generic_subtype_is_returned_when_basetype_is_requested()
        {
            var model = TypeModel.Create();
            Add<MyMessage3>(model);

            Add<JournalEntry<MyMessage3>>(model);
            Add<JournalEntry<Command>>(model);


            DateTime created = DateTime.Now;
            var entry = new JournalEntry<Command>(42, new MyMessage3(43, "dalmatians"), created);
            long bytesWritten;
            var clonedEntry = (JournalEntry<Command>)Clone<JournalEntry<Command>, JournalEntry>(model, entry, out bytesWritten);
            Assert.AreEqual((ulong) 42, clonedEntry.Id);
            Assert.AreEqual(created, clonedEntry.Created);

            var item = clonedEntry.Item as MyMessage3;

            Assert.AreEqual(43,item.MyField);
            Assert.AreEqual("dalmatians", item.MyField2);
            Console.WriteLine("Bytes written:" + bytesWritten);
        }
    }
}
