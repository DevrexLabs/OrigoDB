using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OrigoDB.Core
{
    internal class CustomBinder : SerializationBinder
    {

        private readonly IDictionary<String, Type> _typeMap; 

        public CustomBinder(IDictionary<string, Type> typeMappings)
        {
            _typeMap = typeMappings;
        }

        public override Type BindToType(string assemblyName, string typeName)
        {

            if (_typeMap.ContainsKey(typeName)) return _typeMap[typeName];

            //Same behavior as the default binder
            return Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));
        }
    }
}