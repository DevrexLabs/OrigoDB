using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using OrigoDB.Core.Utilities;

namespace OrigoDB.Core.Migrations
{
    public class CustomBinder : SerializationBinder
    {

        readonly Schema _schema;

        public CustomBinder(Schema schema)
        {
            Ensure.NotNull(schema, "schema");
            _schema = schema;
        }

        public override Type BindToType(string assemblyName, string typeName)
        {

            if (_schema.TypeSubstitutions.ContainsKey(typeName))
            {
                return _schema.TypeSubstitutions[typeName];
            }

            //string[] genericTypeNames;
            
            //if ( IsGeneric(typeName, out genericTypeNames) && genericTypeNames.Any(name => name == typeName))
            //{
            //    Type mainType = this.BindToType(assemblyName, genericTypeNames[0]);
            //    return mainType.MakeGenericType(genericTypeNames.Skip(1).Select(t => BindToType(assemblyName, t)).ToArray());
            //}

            else return Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));
        }

        private bool IsGeneric(string typeName, out string[] types)
        {
            types = null;
            var match = Regex.Match(typeName, @"^(?<first>[.a-z]+)`\d+\[(?<rest>.*)\]$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var typeList = new List<string>();
                typeList.Add(match.Groups["first"].Value);
                foreach(Match subMatch in Regex.Matches(match.Groups["rest"].Value, @"\[(?<name>[^,]+)"))
                {
                    string fullName = subMatch.Groups["name"].Value;
                    typeList.Add(fullName);
                }
                types = typeList.ToArray();
            }

            return match.Success;
        }
    }
}