using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ProtoBuf.Meta;

namespace OrigoDB.Modules.Protobuf
{
    public class RuntimeTypeModelBuilder
    {
        private HashSet<Type> _types;
        private RuntimeTypeModel _typeModel;

        public RuntimeTypeModel TypeModel
        {
            get { return _typeModel; }
        }

        public RuntimeTypeModelBuilder(RuntimeTypeModel typeModel = null)
        {
            _types = new HashSet<Type>();
            _typeModel = typeModel ?? RuntimeTypeModel.Create();
        }

        private class MetaTypeInfo
        {
            public MetaType MetaType;
            public int NumFields;
        }

        /// <summary>
        /// A replacement for RuntimeTypeModel.IsDefined, which is broken
        /// </summary>
        private bool IsDefined(Type theSoughtType)
        {
            foreach (MetaType aDefinedMetaType in _typeModel.GetTypes())
            {
                if (theSoughtType == aDefinedMetaType.Type) return true;
            }
            return false;

        }

        /// <summary>
        /// Register the type and all it's ancestor types
        /// </summary>
        /// <param name="type"></param>
        public bool Add(Type type)
        {
            if (IsKnownType(type)) return false;
            
            //Remember the types in reverse order so we can add associations
            //in the opposite direction
            var inheritanceChain = new Stack<MetaTypeInfo>();
            while (type != typeof(object) && type != typeof(ValueType))
            {
                if (IsDefined(type))
                {
                    //if we are in the middle of the inheritance chain,
                    //we need to add the previous type as a child of the current type
                    int fieldCount = FieldNames(type, false).Length;
                    var subTypes = _typeModel[type].GetSubtypes();
                    if (subTypes != null) fieldCount += subTypes.Length;
                    inheritanceChain.Push(new MetaTypeInfo { NumFields = fieldCount, MetaType = _typeModel[type] });
                    break;
                }

                var metaType = _typeModel.Add(type, false);
                metaType.UseConstructor = false; //Same behavior as BinaryFormatter
                //metaType.AsReferenceDefault = true;
                int numFields;
                AddInferredFields(metaType, out numFields, includeInherited: false);
                inheritanceChain.Push(new MetaTypeInfo { NumFields = numFields, MetaType = metaType });
                type = type.BaseType;
            }
            
            //register each type with it's parent
            var baseType = inheritanceChain.Pop();
            while (inheritanceChain.Count > 0)
            {
                MetaTypeInfo subType = inheritanceChain.Pop();
                baseType.MetaType.AddSubType(baseType.NumFields + 1, subType.MetaType.Type);
                baseType = subType;
            }
            return true;
        }

        public bool IsKnownType(Type type)
        {
            return _types.Contains(type);
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
    }
}
