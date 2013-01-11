using System;
using System.Collections.Generic;

namespace LiveDomain.Core.Migrations
{
    public class Schema
    {
        private static Schema _current;
        public static Schema Current 
        { 
            get
            {
                lock (typeof(Schema))
                {
                    if (_current == null) _current = new Schema();    
                }
                
                return _current;
            } 
        }

        public Dictionary<string, Type> TypeSubstitutions
        {
            get { return _typeSubstitutions; }
        }
        
        private readonly Dictionary<string,Type> _typeSubstitutions  = new Dictionary<string, Type>();
        
        public class SchemaSource
        {
            private readonly string _typeName;
            private readonly Schema _context;

            public void With<T>()
            {
                _context.TypeSubstitutions.Add(_typeName, typeof(T));
            }

            public SchemaSource(string typeName, Schema context = null)
            {
                _context = context ?? Schema.Current;
                _typeName = typeName;
            }
        }


        public static SchemaSource Substitute(string typeName)
        {
            return new SchemaSource(typeName);
        }
    }
}
